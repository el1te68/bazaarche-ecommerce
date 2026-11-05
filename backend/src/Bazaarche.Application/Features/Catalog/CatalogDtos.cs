using System.Linq.Expressions;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;

namespace Bazaarche.Application.Features.Catalog;

public record ProductCardDto(
    int Id,
    string Title,
    string Slug,
    string? ImageUrl,
    long Price,
    long FinalPrice,
    int DiscountPercent,
    double Rating,
    int ReviewCount,
    bool InStock,
    bool IsAmazingOffer,
    DateTime? OfferEndsAt,
    List<string> Colors);

public record CategoryNodeDto(int Id, string Name, string Slug, string? Icon, string? ImageUrl, List<CategoryNodeDto> Children);

public record CategoryLinkDto(int Id, string Name, string Slug);

public record BrandDto(int Id, string Name, string EnglishName, string Slug, string? LogoUrl);

public record BannerDto(int Id, string Title, string ImageUrl, string? LinkUrl, BannerPosition Position);

public record HomeDto(
    List<BannerDto> HeroBanners,
    List<BannerDto> MiddleBanners,
    List<CategoryNodeDto> Categories,
    List<ProductCardDto> AmazingOffers,
    List<ProductCardDto> Bestsellers,
    List<ProductCardDto> Newest,
    List<ProductCardDto> TopDiscounts,
    List<ProductCardDto> TopRated,
    List<BrandDto> Brands);

public record VariantDto(int Id, string? ColorName, string? ColorHex, long Price, long FinalPrice, int DiscountPercent, int Stock);

public record SpecificationDto(string Key, string Value);

public record ProductDetailDto(
    int Id,
    string Title,
    string? EnglishTitle,
    string Slug,
    string Description,
    List<CategoryLinkDto> Breadcrumbs,
    BrandDto? Brand,
    List<string> Images,
    List<VariantDto> Variants,
    List<SpecificationDto> Specifications,
    double Rating,
    int ReviewCount,
    int SoldCount,
    bool IsAmazingOffer,
    DateTime? OfferEndsAt,
    bool IsInWishlist);

public record BrandFacetDto(int Id, string Name, string Slug, int Count);

public record ProductListResponse(
    Common.PagedResult<ProductCardDto> Products,
    CategoryNodeDto? Category,
    List<CategoryLinkDto> Breadcrumbs,
    List<BrandFacetDto> Brands,
    long MaxPrice);

public record SuggestDto(List<SuggestProductDto> Products, List<CategoryLinkDto> Categories);

public record SuggestProductDto(string Title, string Slug, string? ImageUrl);

public class ProductQuery
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    /// <summary>Comma separated brand slugs.</summary>
    public string? Brands { get; set; }
    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public bool InStock { get; set; }
    public bool HasDiscount { get; set; }
    public bool AmazingOnly { get; set; }
    /// <summary>newest | cheapest | expensive | bestselling | popular | rating | discount</summary>
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
}

public static class CatalogProjections
{
    public static readonly Expression<Func<Product, ProductCardDto>> ToCard = p => new ProductCardDto(
        p.Id,
        p.Title,
        p.Slug,
        p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
        p.Price,
        p.FinalPrice,
        p.DiscountPercent,
        p.AverageRating,
        p.ReviewCount,
        p.TotalStock > 0,
        p.IsAmazingOffer,
        p.OfferEndsAt,
        p.Variants.Where(v => v.ColorHex != null).OrderBy(v => v.Id).Select(v => v.ColorHex!).ToList());

    public static BrandDto ToDto(this Brand b) => new(b.Id, b.Name, b.EnglishName, b.Slug, b.LogoUrl);

    public static BannerDto ToDto(this Banner b) => new(b.Id, b.Title, b.ImageUrl, b.LinkUrl, b.Position);
}
