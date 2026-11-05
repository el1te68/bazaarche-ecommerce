using Bazaarche.Application.Common;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Catalog;

public class CatalogService(IAppDbContext db, ICurrentUser currentUser)
{
    private IQueryable<Domain.Entities.Product> Visible => db.Products.AsNoTracking().Where(p => p.IsActive);

    public async Task<HomeDto> GetHomeAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var banners = await db.Banners.AsNoTracking().Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder).ToListAsync(ct);
        var tree = await CategoryTree.LoadAsync(db, true, ct);

        var amazing = await Visible
            .Where(p => p.IsAmazingOffer && p.TotalStock > 0 && (p.OfferEndsAt == null || p.OfferEndsAt > now))
            .OrderByDescending(p => p.DiscountPercent).Take(14)
            .Select(CatalogProjections.ToCard).ToListAsync(ct);
        var bestsellers = await Visible.Where(p => p.TotalStock > 0)
            .OrderByDescending(p => p.SoldCount).Take(12).Select(CatalogProjections.ToCard).ToListAsync(ct);
        var newest = await Visible.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            .Take(12).Select(CatalogProjections.ToCard).ToListAsync(ct);
        var discounts = await Visible.Where(p => p.DiscountPercent > 0 && p.TotalStock > 0)
            .OrderByDescending(p => p.DiscountPercent).Take(12).Select(CatalogProjections.ToCard).ToListAsync(ct);
        var topRated = await Visible.Where(p => p.ReviewCount > 0)
            .OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount)
            .Take(12).Select(CatalogProjections.ToCard).ToListAsync(ct);
        var brands = await db.Brands.AsNoTracking().OrderBy(b => b.Id).Take(16).ToListAsync(ct);

        return new HomeDto(
            banners.Where(b => b.Position == BannerPosition.HeroSlider).Select(b => b.ToDto()).ToList(),
            banners.Where(b => b.Position == BannerPosition.MiddleBanner).Select(b => b.ToDto()).ToList(),
            tree.Roots(),
            amazing, bestsellers, newest, discounts, topRated,
            brands.Select(b => b.ToDto()).ToList());
    }

    public async Task<List<CategoryNodeDto>> GetCategoryTreeAsync(CancellationToken ct) =>
        (await CategoryTree.LoadAsync(db, true, ct)).Roots();

    public async Task<List<BrandDto>> GetBrandsAsync(CancellationToken ct) =>
        (await db.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct)).Select(b => b.ToDto()).ToList();

    public async Task<ProductListResponse> SearchAsync(ProductQuery q, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize);
        var tree = await CategoryTree.LoadAsync(db, true, ct);
        var query = Visible;

        CategoryNodeDto? categoryNode = null;
        var breadcrumbs = new List<CategoryLinkDto>();
        if (!string.IsNullOrWhiteSpace(q.Category))
        {
            var category = tree.FindBySlug(q.Category) ?? throw new NotFoundException("دسته‌بندی پیدا نشد.");
            var ids = tree.DescendantIdsIncludingSelf(category.Id);
            query = query.Where(p => ids.Contains(p.CategoryId));
            categoryNode = tree.ToNode(category);
            breadcrumbs = tree.Breadcrumbs(category.Id);
        }

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim().Replace('ي', 'ی').Replace('ك', 'ک');
            var pattern = $"%{term}%".ToLowerInvariant();
            query = query.Where(p => EF.Functions.Like(p.Title.ToLower(), pattern)
                                     || (p.EnglishTitle != null && EF.Functions.Like(p.EnglishTitle.ToLower(), pattern))
                                     || (p.Brand != null && (EF.Functions.Like(p.Brand.Name.ToLower(), pattern) || EF.Functions.Like(p.Brand.EnglishName.ToLower(), pattern))));
        }

        // Brand facets and price range are computed before brand/price filters so the user can still widen them.
        var brandFacets = await query.Where(p => p.BrandId != null)
            .GroupBy(p => new { p.BrandId, p.Brand!.Name, p.Brand.Slug })
            .Select(g => new BrandFacetDto(g.Key.BrandId!.Value, g.Key.Name, g.Key.Slug, g.Count()))
            .ToListAsync(ct);
        var maxPrice = await query.Select(p => (long?)p.FinalPrice).MaxAsync(ct) ?? 0;

        if (!string.IsNullOrWhiteSpace(q.Brands))
        {
            var slugs = q.Brands.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(p => p.Brand != null && slugs.Contains(p.Brand.Slug));
        }
        if (q.MinPrice is > 0) query = query.Where(p => p.FinalPrice >= q.MinPrice);
        if (q.MaxPrice is > 0) query = query.Where(p => p.FinalPrice <= q.MaxPrice);
        if (q.InStock) query = query.Where(p => p.TotalStock > 0);
        if (q.HasDiscount) query = query.Where(p => p.DiscountPercent > 0);
        if (q.AmazingOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(p => p.IsAmazingOffer && (p.OfferEndsAt == null || p.OfferEndsAt > now));
        }

        // Out-of-stock products always go to the end, like most shops.
        var ordered = query.OrderByDescending(p => p.TotalStock > 0);
        ordered = (q.Sort ?? "newest") switch
        {
            "cheapest" => ordered.ThenBy(p => p.FinalPrice),
            "expensive" => ordered.ThenByDescending(p => p.FinalPrice),
            "bestselling" => ordered.ThenByDescending(p => p.SoldCount),
            "popular" => ordered.ThenByDescending(p => p.ViewCount),
            "rating" => ordered.ThenByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount),
            "discount" => ordered.ThenByDescending(p => p.DiscountPercent),
            _ => ordered.ThenByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
        };

        var total = await query.CountAsync(ct);
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(CatalogProjections.ToCard).ToListAsync(ct);

        return new ProductListResponse(
            new PagedResult<ProductCardDto>(items, total, page, pageSize),
            categoryNode, breadcrumbs,
            brandFacets.OrderByDescending(b => b.Count).ToList(),
            maxPrice);
    }

    public async Task<ProductDetailDto> GetProductAsync(string slug, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Specifications)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct)
            ?? throw new NotFoundException("محصول پیدا نشد.");

        product.ViewCount++;
        await db.SaveChangesAsync(ct);

        var tree = await CategoryTree.LoadAsync(db, false, ct);
        var inWishlist = currentUser.UserId is { } uid &&
                         await db.WishlistItems.AnyAsync(w => w.UserId == uid && w.ProductId == product.Id, ct);

        return new ProductDetailDto(
            product.Id, product.Title, product.EnglishTitle, product.Slug, product.Description,
            tree.Breadcrumbs(product.CategoryId),
            product.Brand?.ToDto(),
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
            product.Variants.OrderBy(v => v.Id)
                .Select(v => new VariantDto(v.Id, v.ColorName, v.ColorHex, v.Price, v.GetFinalPrice(), v.DiscountPercent, v.Stock)).ToList(),
            product.Specifications.OrderBy(s => s.DisplayOrder).Select(s => new SpecificationDto(s.Key, s.Value)).ToList(),
            product.AverageRating, product.ReviewCount, product.SoldCount,
            product.IsAmazingOffer && (product.OfferEndsAt == null || product.OfferEndsAt > DateTime.UtcNow),
            product.OfferEndsAt, inWishlist);
    }

    public async Task<List<ProductCardDto>> GetRelatedAsync(int productId, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct)
                      ?? throw new NotFoundException("محصول پیدا نشد.");
        return await Visible.Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
            .OrderByDescending(p => p.TotalStock > 0).ThenByDescending(p => p.SoldCount)
            .Take(12).Select(CatalogProjections.ToCard).ToListAsync(ct);
    }

    public async Task<SuggestDto> SuggestAsync(string? q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2) return new SuggestDto([], []);
        var pattern = $"%{q.Trim().Replace('ي', 'ی').Replace('ك', 'ک')}%".ToLowerInvariant();
        var products = await Visible
            .Where(p => EF.Functions.Like(p.Title.ToLower(), pattern) || (p.EnglishTitle != null && EF.Functions.Like(p.EnglishTitle.ToLower(), pattern)))
            .OrderByDescending(p => p.SoldCount).Take(6)
            .Select(p => new SuggestProductDto(p.Title, p.Slug, p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()))
            .ToListAsync(ct);
        var categories = await db.Categories.AsNoTracking()
            .Where(c => c.IsActive && EF.Functions.Like(c.Name.ToLower(), pattern)).Take(4)
            .Select(c => new CategoryLinkDto(c.Id, c.Name, c.Slug)).ToListAsync(ct);
        return new SuggestDto(products, categories);
    }
}
