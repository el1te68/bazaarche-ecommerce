using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Application.Features.Shopping;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Admin;

public record AdminUserDto(int Id, string FullName, string PhoneNumber, string? Email, UserRole Role, bool IsActive,
    DateTime CreatedAt, int OrdersCount, long TotalSpent);

public class AdminUserQuery
{
    public string? Q { get; set; }
    public UserRole? Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UpdateUserRequest
{
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
}

public record AdminReviewDto(int Id, int ProductId, string ProductTitle, string ProductSlug, string UserName, int Rating,
    string Title, string Comment, bool IsApproved, DateTime CreatedAt);

public class AdminReviewQuery
{
    /// <summary>pending | approved | (null = all)</summary>
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record AdminBannerDto(int Id, string Title, string ImageUrl, string? LinkUrl, BannerPosition Position, int DisplayOrder, bool IsActive);

public class BannerInput
{
    [Required(ErrorMessage = "عنوان بنر الزامی است."), StringLength(120)] public string Title { get; init; } = string.Empty;
    [StringLength(300)] public string? LinkUrl { get; init; }
    public BannerPosition Position { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public class AdminUserService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<PagedResult<AdminUserDto>> ListAsync(AdminUserQuery q, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize, 100);
        var query = db.Users.AsNoTracking().AsQueryable();
        if (q.Role != null) query = query.Where(u => u.Role == q.Role);
        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim();
            var pattern = $"%{term}%".ToLowerInvariant();
            query = query.Where(u => u.PhoneNumber.Contains(term) || EF.Functions.Like(u.FullName.ToLower(), pattern) || (u.Email != null && u.Email.Contains(term)));
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminUserDto(u.Id, u.FullName, u.PhoneNumber, u.Email, u.Role, u.IsActive, u.CreatedAt,
                u.Orders.Count(o => o.Status != OrderStatus.PendingPayment && o.Status != OrderStatus.Cancelled),
                u.Orders.Where(o => o.Status != OrderStatus.PendingPayment && o.Status != OrderStatus.Cancelled).Sum(o => (long?)o.PayableAmount) ?? 0))
            .ToListAsync(ct);
        return new PagedResult<AdminUserDto>(items, total, page, pageSize);
    }

    public async Task UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new NotFoundException("کاربر پیدا نشد.");
        if (id == currentUser.UserId && (request.Role != UserRole.Admin || !request.IsActive))
            throw new AppException("نمی‌توانید دسترسی ادمین یا وضعیت حساب خودتان را تغییر دهید.");
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public class AdminReviewService(IAppDbContext db)
{
    public async Task<PagedResult<AdminReviewDto>> ListAsync(AdminReviewQuery q, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize, 100);
        var query = db.Reviews.AsNoTracking().AsQueryable();
        if (q.Status == "pending") query = query.Where(r => !r.IsApproved);
        else if (q.Status == "approved") query = query.Where(r => r.IsApproved);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(r => r.IsApproved).ThenByDescending(r => r.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new AdminReviewDto(r.Id, r.ProductId, r.Product!.Title, r.Product.Slug, r.User!.FullName, r.Rating,
                r.Title, r.Comment, r.IsApproved, r.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<AdminReviewDto>(items, total, page, pageSize);
    }

    public async Task SetApprovedAsync(int id, bool approved, CancellationToken ct)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw new NotFoundException("نظر پیدا نشد.");
        review.IsApproved = approved;
        review.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await ReviewService.RefreshProductRatingAsync(db, review.ProductId, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw new NotFoundException("نظر پیدا نشد.");
        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
        await ReviewService.RefreshProductRatingAsync(db, review.ProductId, ct);
        await db.SaveChangesAsync(ct);
    }
}

public class AdminBannerService(IAppDbContext db, IFileStorage storage)
{
    private static AdminBannerDto ToDto(Banner b) => new(b.Id, b.Title, b.ImageUrl, b.LinkUrl, b.Position, b.DisplayOrder, b.IsActive);

    public async Task<List<AdminBannerDto>> ListAsync(CancellationToken ct) =>
        (await db.Banners.AsNoTracking().OrderBy(b => b.Position).ThenBy(b => b.DisplayOrder).ToListAsync(ct)).Select(ToDto).ToList();

    public async Task<AdminBannerDto> CreateAsync(BannerInput input, FileUpload image, CancellationToken ct)
    {
        var banner = new Banner { ImageUrl = await storage.SaveAsync(image, "banners", ct) };
        Apply(banner, input);
        db.Banners.Add(banner);
        await db.SaveChangesAsync(ct);
        return ToDto(banner);
    }

    public async Task<AdminBannerDto> UpdateAsync(int id, BannerInput input, FileUpload? image, CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException("بنر پیدا نشد.");
        Apply(banner, input);
        string? old = null;
        if (image != null)
        {
            old = banner.ImageUrl;
            banner.ImageUrl = await storage.SaveAsync(image, "banners", ct);
        }
        banner.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        storage.Delete(old);
        return ToDto(banner);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var banner = await db.Banners.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException("بنر پیدا نشد.");
        db.Banners.Remove(banner);
        await db.SaveChangesAsync(ct);
        storage.Delete(banner.ImageUrl);
    }

    private static void Apply(Banner b, BannerInput input)
    {
        b.Title = input.Title.Trim();
        b.LinkUrl = string.IsNullOrWhiteSpace(input.LinkUrl) ? null : input.LinkUrl.Trim();
        b.Position = input.Position;
        b.DisplayOrder = input.DisplayOrder;
        b.IsActive = input.IsActive;
    }
}
