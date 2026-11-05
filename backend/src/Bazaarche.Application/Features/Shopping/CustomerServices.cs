using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Application.Features.Catalog;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Shopping;

// ---------------- Wishlist ----------------

public class WishlistService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<List<ProductCardDto>> GetAsync(CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        return await db.WishlistItems.AsNoTracking().Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.Product!).Select(CatalogProjections.ToCard).ToListAsync(ct);
    }

    public async Task AddAsync(int productId, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        if (!await db.Products.AnyAsync(p => p.Id == productId, ct)) throw new NotFoundException("محصول پیدا نشد.");
        if (await db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId, ct)) return;
        db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int productId, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var item = await db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
        if (item == null) return;
        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}

// ---------------- Addresses ----------------

public class AddressRequest
{
    [Required(ErrorMessage = "عنوان آدرس الزامی است."), StringLength(40)]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "نام گیرنده الزامی است."), StringLength(80)]
    public string ReceiverName { get; init; } = string.Empty;

    [Required(ErrorMessage = "موبایل گیرنده الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "موبایل گیرنده معتبر نیست.")]
    public string ReceiverPhone { get; init; } = string.Empty;

    [Required(ErrorMessage = "استان الزامی است."), StringLength(40)]
    public string Province { get; init; } = string.Empty;

    [Required(ErrorMessage = "شهر الزامی است."), StringLength(40)]
    public string City { get; init; } = string.Empty;

    [Required(ErrorMessage = "آدرس الزامی است."), StringLength(400, MinimumLength = 10, ErrorMessage = "آدرس را کامل‌تر وارد کنید.")]
    public string FullAddress { get; init; } = string.Empty;

    [Required(ErrorMessage = "کد پستی الزامی است.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد پستی باید ۱۰ رقم باشد.")]
    public string PostalCode { get; init; } = string.Empty;

    public bool IsDefault { get; init; }
}

public record AddressDto(int Id, string Title, string ReceiverName, string ReceiverPhone, string Province, string City,
    string FullAddress, string PostalCode, bool IsDefault);

public class AddressService(IAppDbContext db, ICurrentUser currentUser)
{
    private static AddressDto ToDto(Address a) =>
        new(a.Id, a.Title, a.ReceiverName, a.ReceiverPhone, a.Province, a.City, a.FullAddress, a.PostalCode, a.IsDefault);

    public async Task<List<AddressDto>> GetAllAsync(CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var list = await db.Addresses.AsNoTracking().Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.Id).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<AddressDto> CreateAsync(AddressRequest r, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var all = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        var address = new Address { UserId = userId };
        Apply(address, r);
        address.IsDefault = r.IsDefault || all.Count == 0;
        if (address.IsDefault) all.ForEach(a => a.IsDefault = false);
        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);
        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(int id, AddressRequest r, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var all = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        var address = all.FirstOrDefault(a => a.Id == id) ?? throw new NotFoundException("آدرس پیدا نشد.");
        Apply(address, r);
        if (r.IsDefault)
        {
            all.ForEach(a => a.IsDefault = false);
            address.IsDefault = true;
        }
        address.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDto(address);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var all = await db.Addresses.Where(a => a.UserId == userId).ToListAsync(ct);
        var address = all.FirstOrDefault(a => a.Id == id) ?? throw new NotFoundException("آدرس پیدا نشد.");
        db.Addresses.Remove(address);
        if (address.IsDefault && all.FirstOrDefault(a => a.Id != id) is { } next) next.IsDefault = true;
        await db.SaveChangesAsync(ct);
    }

    private static void Apply(Address a, AddressRequest r)
    {
        a.Title = r.Title.Trim();
        a.ReceiverName = r.ReceiverName.Trim();
        a.ReceiverPhone = r.ReceiverPhone.Trim();
        a.Province = r.Province.Trim();
        a.City = r.City.Trim();
        a.FullAddress = r.FullAddress.Trim();
        a.PostalCode = r.PostalCode.Trim();
    }
}

// ---------------- Reviews ----------------

public class CreateReviewRequest
{
    [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
    public int Rating { get; init; }

    [Required(ErrorMessage = "عنوان نظر الزامی است."), StringLength(100)]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "متن نظر الزامی است."), StringLength(2000, MinimumLength = 5, ErrorMessage = "متن نظر خیلی کوتاه است.")]
    public string Comment { get; init; } = string.Empty;
}

public record ReviewDto(int Id, string UserName, int Rating, string Title, string Comment, DateTime CreatedAt, bool IsBuyer);

public record ReviewSummaryDto(double Average, int Count, Dictionary<int, int> Distribution, PagedResult<ReviewDto> Reviews);

public class ReviewService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<ReviewSummaryDto> GetForProductAsync(int productId, int page, CancellationToken ct)
    {
        (page, var pageSize) = Paging.Normalize(page, 10);
        var approved = db.Reviews.AsNoTracking().Where(r => r.ProductId == productId && r.IsApproved);
        var distribution = await approved.GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() }).ToListAsync(ct);
        var total = distribution.Sum(d => d.Count);
        var avg = total == 0 ? 0 : Math.Round(distribution.Sum(d => d.Rating * d.Count) / (double)total, 1);

        var items = await approved.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReviewDto(r.Id, r.User!.FullName, r.Rating, r.Title, r.Comment, r.CreatedAt,
                db.OrderItems.Any(oi => oi.ProductId == r.ProductId && oi.Order!.UserId == r.UserId &&
                                        oi.Order.Status != OrderStatus.PendingPayment && oi.Order.Status != OrderStatus.Cancelled)))
            .ToListAsync(ct);

        var dict = Enumerable.Range(1, 5).ToDictionary(i => i, i => distribution.FirstOrDefault(d => d.Rating == i)?.Count ?? 0);
        return new ReviewSummaryDto(avg, total, dict, new PagedResult<ReviewDto>(items, total, page, pageSize));
    }

    public async Task CreateAsync(int productId, CreateReviewRequest request, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        if (!await db.Products.AnyAsync(p => p.Id == productId && p.IsActive, ct)) throw new NotFoundException("محصول پیدا نشد.");
        if (await db.Reviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId, ct))
            throw new AppException("شما قبلاً برای این محصول نظر ثبت کرده‌اید.");

        db.Reviews.Add(new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Title = request.Title.Trim(),
            Comment = request.Comment.Trim(),
            IsApproved = false
        });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Recomputes the cached rating of a product from its approved reviews.</summary>
    public static async Task RefreshProductRatingAsync(IAppDbContext db, int productId, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product == null) return;
        var ratings = await db.Reviews.Where(r => r.ProductId == productId && r.IsApproved).Select(r => r.Rating).ToListAsync(ct);
        product.ReviewCount = ratings.Count;
        product.AverageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 1);
    }
}
