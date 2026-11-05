using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Shopping;

public record CartLine(int VariantId, int Quantity);

public class AddToCartRequest
{
    [Range(1, int.MaxValue)] public int VariantId { get; init; }
    [Range(1, 100)] public int Quantity { get; init; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(0, 100)] public int Quantity { get; init; }
}

public record CartItemDto(
    int VariantId,
    int ProductId,
    string Title,
    string Slug,
    string? ImageUrl,
    string? ColorName,
    string? ColorHex,
    long Price,
    long FinalPrice,
    int Quantity,
    int Stock,
    int MaxQuantity);

public record CartDto(List<CartItemDto> Items, int TotalQuantity, long ItemsTotal, long ProductsDiscount, long PayableTotal);

public class CartService(IAppDbContext db, ICurrentUser currentUser)
{
    /// <summary>Max units of a single variant a customer can buy in one order.</summary>
    public const int MaxPerItem = 10;

    public async Task<CartDto> GetAsync(CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var lines = await db.CartItems.AsNoTracking().Where(c => c.UserId == userId)
            .OrderBy(c => c.Id).Select(c => new CartLine(c.ProductVariantId, c.Quantity)).ToListAsync(ct);
        return await BuildAsync(lines, ct);
    }

    /// <summary>Builds a priced cart for a guest (lines stored in the browser).</summary>
    public Task<CartDto> PreviewAsync(List<CartLine> lines, CancellationToken ct) => BuildAsync(lines, ct);

    public async Task<CartDto> AddAsync(AddToCartRequest request, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var variant = await LoadVariantAsync(request.VariantId, ct);
        var item = await db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductVariantId == variant.Id, ct);
        var newQty = (item?.Quantity ?? 0) + request.Quantity;
        EnsureAvailable(variant, newQty);

        if (item == null) db.CartItems.Add(new CartItem { UserId = userId, ProductVariantId = variant.Id, Quantity = newQty });
        else { item.Quantity = newQty; item.UpdatedAt = DateTime.UtcNow; }
        await db.SaveChangesAsync(ct);
        return await GetAsync(ct);
    }

    public async Task<CartDto> UpdateAsync(int variantId, UpdateCartItemRequest request, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var item = await db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductVariantId == variantId, ct)
                   ?? throw new NotFoundException("این کالا در سبد خرید نیست.");
        if (request.Quantity <= 0) db.CartItems.Remove(item);
        else
        {
            var variant = await LoadVariantAsync(variantId, ct);
            EnsureAvailable(variant, request.Quantity);
            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(ct);
    }

    public async Task<CartDto> RemoveAsync(int variantId, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var item = await db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductVariantId == variantId, ct);
        if (item != null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
        return await GetAsync(ct);
    }

    public async Task ClearAsync(CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var items = await db.CartItems.Where(c => c.UserId == userId).ToListAsync(ct);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Merges the guest cart (from the browser) into the user's cart after login.</summary>
    public async Task<CartDto> MergeAsync(List<CartLine> lines, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var existing = await db.CartItems.Where(c => c.UserId == userId).ToListAsync(ct);
        var variantIds = lines.Select(l => l.VariantId).Distinct().ToList();
        var variants = await db.ProductVariants.AsNoTracking()
            .Where(v => variantIds.Contains(v.Id) && v.Product!.IsActive).ToDictionaryAsync(v => v.Id, ct);

        foreach (var line in lines.Where(l => l.Quantity > 0))
        {
            if (!variants.TryGetValue(line.VariantId, out var variant) || variant.Stock <= 0) continue;
            var item = existing.FirstOrDefault(e => e.ProductVariantId == line.VariantId);
            var qty = Math.Min(Math.Max(item?.Quantity ?? 0, line.Quantity), Math.Min(variant.Stock, MaxPerItem));
            if (item == null)
            {
                item = new CartItem { UserId = userId, ProductVariantId = line.VariantId, Quantity = qty };
                db.CartItems.Add(item);
                existing.Add(item);
            }
            else item.Quantity = qty;
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(ct);
    }

    private async Task<ProductVariant> LoadVariantAsync(int variantId, CancellationToken ct) =>
        await db.ProductVariants.Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.Product!.IsActive, ct)
        ?? throw new NotFoundException("کالا پیدا نشد.");

    private static void EnsureAvailable(ProductVariant variant, int quantity)
    {
        if (variant.Stock <= 0) throw new AppException("این کالا ناموجود است.");
        if (quantity > variant.Stock) throw new AppException($"فقط {variant.Stock} عدد از این کالا موجود است.");
        if (quantity > MaxPerItem) throw new AppException($"حداکثر {MaxPerItem} عدد از هر کالا قابل سفارش است.");
    }

    private async Task<CartDto> BuildAsync(List<CartLine> lines, CancellationToken ct)
    {
        var ids = lines.Select(l => l.VariantId).Distinct().ToList();
        var variants = await db.ProductVariants.AsNoTracking()
            .Where(v => ids.Contains(v.Id) && v.Product!.IsActive)
            .Select(v => new
            {
                v.Id, v.ProductId, v.Product!.Title, v.Product.Slug,
                Image = v.Product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                v.ColorName, v.ColorHex, v.Price, v.DiscountPercent, v.Stock
            }).ToListAsync(ct);

        var items = new List<CartItemDto>();
        foreach (var line in lines)
        {
            var v = variants.FirstOrDefault(x => x.Id == line.VariantId);
            if (v == null || line.Quantity <= 0) continue;
            var finalPrice = v.Price - v.Price * v.DiscountPercent / 100;
            items.Add(new CartItemDto(v.Id, v.ProductId, v.Title, v.Slug, v.Image, v.ColorName, v.ColorHex,
                v.Price, finalPrice, line.Quantity, v.Stock, Math.Min(v.Stock, MaxPerItem)));
        }

        var available = items.Where(i => i.Stock > 0).ToList();
        var itemsTotal = available.Sum(i => i.Price * Math.Min(i.Quantity, i.Stock));
        var payable = available.Sum(i => i.FinalPrice * Math.Min(i.Quantity, i.Stock));
        return new CartDto(items, available.Sum(i => i.Quantity), itemsTotal, itemsTotal - payable, payable);
    }
}
