using System.Security.Cryptography;
using Bazaarche.Application.Common;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Orders;

public class OrderService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<List<OrderSummaryDto>> GetMyOrdersAsync(OrderStatus? status, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var query = db.Orders.AsNoTracking().Where(o => o.UserId == userId);
        if (status != null) query = query.Where(o => o.Status == status);
        return await query.OrderByDescending(o => o.Id)
            .Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status, o.CreatedAt, o.PayableAmount,
                o.Items.Sum(i => i.Quantity), o.Items.OrderBy(i => i.Id).Select(i => i.ImageUrl).Take(4).ToList()))
            .ToListAsync(ct);
    }

    public async Task<OrderDetailDto> GetMyOrderAsync(int id, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var order = await db.Orders.AsNoTracking().Include(o => o.Items).Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
                    ?? throw new NotFoundException("سفارش پیدا نشد.");
        return order.ToDetail();
    }

    public async Task CancelAsync(int id, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
                    ?? throw new NotFoundException("سفارش پیدا نشد.");
        if (order.Status != OrderStatus.PendingPayment)
            throw new AppException("فقط سفارش‌های در انتظار پرداخت قابل لغو هستند. برای سایر موارد با پشتیبانی تماس بگیرید.");
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await CheckoutService.RestockAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Creates a new payment attempt for an unpaid order.</summary>
    public async Task<CheckoutResultDto> RetryPaymentAsync(int id, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
                    ?? throw new NotFoundException("سفارش پیدا نشد.");
        if (order.Status != OrderStatus.PendingPayment) throw new AppException("این سفارش در انتظار پرداخت نیست.");
        var payment = CheckoutService.NewPayment(order.PayableAmount);
        payment.OrderId = order.Id;
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return new CheckoutResultDto(order.Id, order.OrderNumber, payment.Token, order.PayableAmount);
    }
}

/// <summary>A simulated bank gateway. Works like a real one: the shop creates a token, the customer pays on the
/// gateway page, and the gateway reports back the result.</summary>
public class PaymentService(IAppDbContext db)
{
    public const string MerchantName = "فروشگاه اینترنتی بازارچه";

    public async Task<GatewayInfoDto> GetAsync(string token, CancellationToken ct)
    {
        var payment = await db.Payments.AsNoTracking().Include(p => p.Order)
                          .FirstOrDefaultAsync(p => p.Token == token, ct)
                      ?? throw new NotFoundException("تراکنش پیدا نشد.");
        return new GatewayInfoDto(payment.Token, payment.Order!.OrderNumber, payment.Amount, MerchantName, payment.Status, payment.CreatedAt);
    }

    public async Task<PaymentResultDto> CompleteAsync(string token, CompletePaymentRequest request, CancellationToken ct)
    {
        var payment = await db.Payments.Include(p => p.Order!).ThenInclude(o => o.Items)
                          .FirstOrDefaultAsync(p => p.Token == token, ct)
                      ?? throw new NotFoundException("تراکنش پیدا نشد.");
        var order = payment.Order!;

        if (payment.Status != PaymentStatus.Pending)
            return new PaymentResultDto(payment.Status == PaymentStatus.Succeeded, order.Id, order.OrderNumber, payment.Amount,
                payment.ReferenceId, "این تراکنش قبلاً پردازش شده است.");

        if (order.Status != OrderStatus.PendingPayment)
        {
            payment.Status = PaymentStatus.Failed;
            payment.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return new PaymentResultDto(false, order.Id, order.OrderNumber, payment.Amount, null, "این سفارش دیگر قابل پرداخت نیست.");
        }

        payment.CompletedAt = DateTime.UtcNow;
        payment.CardMask = MaskCard(request.CardNumber);

        if (!request.Success)
        {
            payment.Status = PaymentStatus.Failed;
            await db.SaveChangesAsync(ct);
            return new PaymentResultDto(false, order.Id, order.OrderNumber, payment.Amount, null,
                "پرداخت ناموفق بود. در صورت کسر وجه، مبلغ تا ۷۲ ساعت آینده به حساب شما برمی‌گردد.");
        }

        payment.Status = PaymentStatus.Succeeded;
        payment.ReferenceId = RandomNumberGenerator.GetInt32(100_000_000, 999_999_999).ToString() +
                              RandomNumberGenerator.GetInt32(100, 999);
        order.Status = OrderStatus.Processing;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Statistics: sold count and coupon usage.
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        foreach (var product in products)
            product.SoldCount += order.Items.Where(i => i.ProductId == product.Id).Sum(i => i.Quantity);
        if (order.CouponCode != null && await db.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode, ct) is { } coupon)
            coupon.UsedCount++;

        await db.SaveChangesAsync(ct);
        return new PaymentResultDto(true, order.Id, order.OrderNumber, payment.Amount, payment.ReferenceId, "پرداخت با موفقیت انجام شد.");
    }

    private static string? MaskCard(string? card)
    {
        var digits = new string((card ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length < 10 ? null : $"{digits[..6]}******{digits[^4..]}";
    }
}
