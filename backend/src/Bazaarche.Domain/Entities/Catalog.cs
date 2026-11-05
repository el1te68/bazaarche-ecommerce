using Bazaarche.Domain.Common;

namespace Bazaarche.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    /// <summary>Name of a lucide icon used by the storefront (e.g. "smartphone").</summary>
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public List<Product> Products { get; set; } = [];
}

public class Product : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? EnglishTitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    // "Amazing offer" (شگفت‌انگیز)
    public bool IsAmazingOffer { get; set; }
    public DateTime? OfferEndsAt { get; set; }

    // Statistics
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Denormalized summary of the variants (kept in sync by RecalculateSummary) for fast filtering/sorting.
    public long Price { get; set; }
    public long FinalPrice { get; set; }
    public int DiscountPercent { get; set; }
    public int TotalStock { get; set; }

    public List<ProductImage> Images { get; set; } = [];
    public List<ProductVariant> Variants { get; set; } = [];
    public List<ProductSpecification> Specifications { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];

    /// <summary>Recomputes the price/stock summary from the variants. The cheapest in-stock variant wins.</summary>
    public void RecalculateSummary()
    {
        var active = Variants.Where(v => v.Stock > 0).ToList();
        var pool = active.Count > 0 ? active : Variants;
        var best = pool.OrderBy(v => v.GetFinalPrice()).FirstOrDefault();
        Price = best?.Price ?? 0;
        FinalPrice = best?.GetFinalPrice() ?? 0;
        DiscountPercent = best?.DiscountPercent ?? 0;
        TotalStock = Variants.Sum(v => v.Stock);
    }
}

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>Null when the product has no color options.</summary>
    public string? ColorName { get; set; }
    public string? ColorHex { get; set; }
    public string? Sku { get; set; }
    public long Price { get; set; }
    public int DiscountPercent { get; set; }
    public int Stock { get; set; }

    public long GetFinalPrice() => Price - (Price * DiscountPercent / 100);
}

public class ProductSpecification : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public Enums.BannerPosition Position { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
