using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Application.Features.Orders;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Admin;

public record AdminOrderRowDto(int Id, string OrderNumber, string CustomerName, string CustomerPhone, OrderStatus Status,
    long PayableAmount, int ItemsCount, ShippingMethod ShippingMethod, DateTime CreatedAt, DateTime? PaidAt);

public class AdminOrderQuery
{
    public string? Q { get; set; }
    public OrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; init; }
    [StringLength(60)] public string? TrackingCode { get; init; }
}

public record CouponDto(int Id, string Code, CouponType Type, long Value, long? MaxDiscountAmount, long MinOrderAmount,
    DateTime? ExpiresAt, int? UsageLimit, int UsedCount, bool IsActive, DateTime CreatedAt);

public class CouponInput
{
    [Required(ErrorMessage = "کد تخفیف الزامی است.")]
    [RegularExpression("^[A-Za-z0-9_-]{3,30}$", ErrorMessage = "کد فقط شامل حروف انگلیسی، عدد، - و _ (۳ تا ۳۰ کاراکتر) باشد.")]
    public string Code { get; init; } = string.Empty;
    public CouponType Type { get; init; }
    [Range(1, 1_000_000_000, ErrorMessage = "مقدار تخفیف معتبر نیست.")] public long Value { get; init; }
    public long? MaxDiscountAmount { get; init; }
    [Range(0, 10_000_000_000)] public long MinOrderAmount { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int? UsageLimit { get; init; }
    public bool IsActive { get; init; } = true;
}

public class AdminOrderService(IAppDbContext db)
{
    // Allowed manual transitions for the admin.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Transitions = new()
    {
        [OrderStatus.PendingPayment] = [OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    public async Task<PagedResult<AdminOrderRowDto>> ListAsync(AdminOrderQuery q, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize, 100);
        var query = db.Orders.AsNoTracking().AsQueryable();
        if (q.Status != null) query = query.Where(o => o.Status == q.Status);
        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim();
            var pattern = $"%{term}%".ToLowerInvariant();
            query = query.Where(o => o.OrderNumber.Contains(term) || o.User!.PhoneNumber.Contains(term) || EF.Functions.Like(o.User.FullName.ToLower(), pattern));
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(o => o.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new AdminOrderRowDto(o.Id, o.OrderNumber, o.User!.FullName, o.User.PhoneNumber, o.Status, o.PayableAmount,
                o.Items.Sum(i => i.Quantity), o.ShippingMethod, o.CreatedAt, o.PaidAt))
            .ToListAsync(ct);
        return new PagedResult<AdminOrderRowDto>(items, total, page, pageSize);
    }

    public async Task<OrderDetailDto> GetAsync(int id, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Payments).Include(o => o.User)
                        .AsSplitQuery().FirstOrDefaultAsync(o => o.Id == id, ct)
                    ?? throw new NotFoundException("سفارش پیدا نشد.");
        return order.ToDetail(includeCustomer: true);
    }

    public async Task<OrderDetailDto> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct)
                    ?? throw new NotFoundException("سفارش پیدا نشد.");

        if (request.Status != order.Status)
        {
            if (!Transitions[order.Status].Contains(request.Status))
                throw new AppException("تغییر وضعیت سفارش به این حالت مجاز نیست.");
            if (request.Status == OrderStatus.Cancelled)
            {
                await CheckoutService.RestockAsync(db, order, ct);
                if (order.Status != OrderStatus.PendingPayment)
                {
                    // Paid order cancelled by the shop: roll back the sales statistics.
                    var ids = order.Items.Select(i => i.ProductId).Distinct().ToList();
                    var products = await db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);
                    foreach (var p in products)
                        p.SoldCount = Math.Max(0, p.SoldCount - order.Items.Where(i => i.ProductId == p.Id).Sum(i => i.Quantity));
                }
            }
            order.Status = request.Status;
        }
        if (request.TrackingCode != null) order.TrackingCode = string.IsNullOrWhiteSpace(request.TrackingCode) ? null : request.TrackingCode.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }
}

public class AdminCouponService(IAppDbContext db)
{
    private static CouponDto ToDto(Coupon c) => new(c.Id, c.Code, c.Type, c.Value, c.MaxDiscountAmount, c.MinOrderAmount,
        c.ExpiresAt, c.UsageLimit, c.UsedCount, c.IsActive, c.CreatedAt);

    public async Task<List<CouponDto>> ListAsync(CancellationToken ct) =>
        (await db.Coupons.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync(ct)).Select(ToDto).ToList();

    public async Task<CouponDto> CreateAsync(CouponInput input, CancellationToken ct)
    {
        var coupon = new Coupon();
        await ApplyAsync(coupon, input, ct);
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync(ct);
        return ToDto(coupon);
    }

    public async Task<CouponDto> UpdateAsync(int id, CouponInput input, CancellationToken ct)
    {
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw new NotFoundException("کد تخفیف پیدا نشد.");
        await ApplyAsync(coupon, input, ct);
        coupon.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDto(coupon);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw new NotFoundException("کد تخفیف پیدا نشد.");
        db.Coupons.Remove(coupon);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Coupon c, CouponInput input, CancellationToken ct)
    {
        var code = input.Code.Trim().ToUpperInvariant();
        if (await db.Coupons.AnyAsync(x => x.Code == code && x.Id != c.Id, ct)) throw new AppException("این کد تخفیف قبلاً ساخته شده است.");
        if (input.Type == CouponType.Percent && input.Value > 100) throw new AppException("درصد تخفیف نمی‌تواند بیشتر از ۱۰۰ باشد.");
        c.Code = code;
        c.Type = input.Type;
        c.Value = input.Value;
        c.MaxDiscountAmount = input.MaxDiscountAmount is > 0 ? input.MaxDiscountAmount : null;
        c.MinOrderAmount = input.MinOrderAmount;
        c.ExpiresAt = input.ExpiresAt?.ToUniversalTime();
        c.UsageLimit = input.UsageLimit is > 0 ? input.UsageLimit : null;
        c.IsActive = input.IsActive;
    }
}
