using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Application.Features.Catalog;
using Bazaarche.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Admin;

// ---------------- DTOs ----------------

public record AdminProductRowDto(int Id, string Title, string Slug, string? ImageUrl, string CategoryName, string? BrandName,
    long FinalPrice, int DiscountPercent, int TotalStock, int SoldCount, bool IsActive, bool IsAmazingOffer, DateTime CreatedAt);

public record AdminImageDto(int Id, string Url, int DisplayOrder);

public record AdminVariantDto(int Id, string? ColorName, string? ColorHex, string? Sku, long Price, int DiscountPercent, int Stock);

public record AdminProductDto(int Id, string Title, string? EnglishTitle, string Slug, string Description, int CategoryId, int? BrandId,
    bool IsActive, bool IsAmazingOffer, DateTime? OfferEndsAt, List<AdminImageDto> Images, List<AdminVariantDto> Variants,
    List<SpecificationDto> Specifications);

public class VariantInput
{
    public int? Id { get; init; }
    [StringLength(40)] public string? ColorName { get; init; }
    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "کد رنگ باید به شکل #RRGGBB باشد.")]
    public string? ColorHex { get; init; }
    [StringLength(60)] public string? Sku { get; init; }
    [Range(1000, 10_000_000_000, ErrorMessage = "قیمت معتبر نیست.")] public long Price { get; init; }
    [Range(0, 99, ErrorMessage = "درصد تخفیف باید بین ۰ تا ۹۹ باشد.")] public int DiscountPercent { get; init; }
    [Range(0, 100_000)] public int Stock { get; init; }
}

public class SpecInput
{
    [Required, StringLength(80)] public string Key { get; init; } = string.Empty;
    [Required, StringLength(300)] public string Value { get; init; } = string.Empty;
}

public class ProductInput
{
    [Required(ErrorMessage = "عنوان محصول الزامی است."), StringLength(200)]
    public string Title { get; init; } = string.Empty;
    [StringLength(200)] public string? EnglishTitle { get; init; }
    [StringLength(140)] public string? Slug { get; init; }
    [Required(ErrorMessage = "توضیحات الزامی است.")] public string Description { get; init; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "دسته‌بندی را انتخاب کنید.")] public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsAmazingOffer { get; init; }
    public DateTime? OfferEndsAt { get; init; }
    [MinLength(1, ErrorMessage = "حداقل یک تنوع (قیمت/موجودی) لازم است.")]
    public List<VariantInput> Variants { get; init; } = [];
    public List<SpecInput> Specifications { get; init; } = [];
}

public class AdminProductQuery
{
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool LowStock { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record AdminCategoryDto(int Id, string Name, string Slug, string? Icon, string? ImageUrl, int? ParentId, string? ParentName,
    int DisplayOrder, bool IsActive, int ProductCount);

public class CategoryInput
{
    [Required(ErrorMessage = "نام دسته‌بندی الزامی است."), StringLength(80)] public string Name { get; init; } = string.Empty;
    [StringLength(100)] public string? Slug { get; init; }
    [StringLength(40)] public string? Icon { get; init; }
    public int? ParentId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public record AdminBrandDto(int Id, string Name, string EnglishName, string Slug, string? LogoUrl, int ProductCount);

public class BrandInput
{
    [Required(ErrorMessage = "نام برند الزامی است."), StringLength(80)] public string Name { get; init; } = string.Empty;
    [Required(ErrorMessage = "نام انگلیسی برند الزامی است."), StringLength(80)] public string EnglishName { get; init; } = string.Empty;
    [StringLength(100)] public string? Slug { get; init; }
}

// ---------------- Products ----------------

public class AdminProductService(IAppDbContext db, IFileStorage storage)
{
    public async Task<PagedResult<AdminProductRowDto>> ListAsync(AdminProductQuery q, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(q.Page, q.PageSize, 100);
        var query = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var pattern = $"%{q.Q.Trim()}%".ToLowerInvariant();
            query = query.Where(p => EF.Functions.Like(p.Title.ToLower(), pattern) || (p.EnglishTitle != null && EF.Functions.Like(p.EnglishTitle.ToLower(), pattern)));
        }
        if (q.CategoryId != null) query = query.Where(p => p.CategoryId == q.CategoryId);
        if (q.IsActive != null) query = query.Where(p => p.IsActive == q.IsActive);
        if (q.LowStock) query = query.Where(p => p.TotalStock <= 5);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new AdminProductRowDto(p.Id, p.Title, p.Slug,
                p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                p.Category!.Name, p.Brand != null ? p.Brand.Name : null, p.FinalPrice, p.DiscountPercent, p.TotalStock,
                p.SoldCount, p.IsActive, p.IsAmazingOffer, p.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<AdminProductRowDto>(items, total, page, pageSize);
    }

    public async Task<AdminProductDto> GetAsync(int id, CancellationToken ct)
    {
        var p = await Load(id, ct);
        return ToDto(p);
    }

    public async Task<AdminProductDto> CreateAsync(ProductInput input, CancellationToken ct)
    {
        await ValidateReferencesAsync(input, ct);
        var product = new Product();
        await ApplyAsync(product, input, ct);
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<AdminProductDto> UpdateAsync(int id, ProductInput input, CancellationToken ct)
    {
        await ValidateReferencesAsync(input, ct);
        var product = await Load(id, ct);
        await ApplyAsync(product, input, ct);
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await Load(id, ct);
        var urls = product.Images.Select(i => i.Url).ToList();
        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        urls.ForEach(storage.Delete);
    }

    public async Task<List<AdminImageDto>> AddImagesAsync(int id, IEnumerable<FileUpload> files, CancellationToken ct)
    {
        var product = await Load(id, ct);
        var order = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.DisplayOrder) + 1;
        foreach (var file in files)
        {
            var url = await storage.SaveAsync(file, "products", ct);
            product.Images.Add(new ProductImage { Url = url, DisplayOrder = order++ });
        }
        await db.SaveChangesAsync(ct);
        return ToDto(product).Images;
    }

    public async Task<List<AdminImageDto>> DeleteImageAsync(int id, int imageId, CancellationToken ct)
    {
        var product = await Load(id, ct);
        var image = product.Images.FirstOrDefault(i => i.Id == imageId) ?? throw new NotFoundException("تصویر پیدا نشد.");
        product.Images.Remove(image);
        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(ct);
        storage.Delete(image.Url);
        return ToDto(product).Images;
    }

    /// <summary>Reorders images; the first one becomes the main image.</summary>
    public async Task<List<AdminImageDto>> ReorderImagesAsync(int id, List<int> imageIds, CancellationToken ct)
    {
        var product = await Load(id, ct);
        for (var i = 0; i < imageIds.Count; i++)
            if (product.Images.FirstOrDefault(x => x.Id == imageIds[i]) is { } img) img.DisplayOrder = i;
        await db.SaveChangesAsync(ct);
        return ToDto(product).Images;
    }

    private async Task<Product> Load(int id, CancellationToken ct) =>
        await db.Products.Include(p => p.Images).Include(p => p.Variants).Include(p => p.Specifications)
            .AsSplitQuery().FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NotFoundException("محصول پیدا نشد.");

    private async Task ValidateReferencesAsync(ProductInput input, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == input.CategoryId, ct)) throw new AppException("دسته‌بندی انتخاب‌شده وجود ندارد.");
        if (input.BrandId != null && !await db.Brands.AnyAsync(b => b.Id == input.BrandId, ct)) throw new AppException("برند انتخاب‌شده وجود ندارد.");
        if (input.Variants.Count == 0) throw new AppException("حداقل یک تنوع (قیمت/موجودی) لازم است.");
        if (input.IsAmazingOffer && input.Variants.All(v => v.DiscountPercent == 0))
            throw new AppException("برای پیشنهاد شگفت‌انگیز، حداقل یک تنوع باید تخفیف داشته باشد.");
    }

    private async Task ApplyAsync(Product p, ProductInput input, CancellationToken ct)
    {
        p.Title = input.Title.Trim();
        p.EnglishTitle = string.IsNullOrWhiteSpace(input.EnglishTitle) ? null : input.EnglishTitle.Trim();
        p.Description = input.Description.Trim();
        p.CategoryId = input.CategoryId;
        p.BrandId = input.BrandId;
        p.IsActive = input.IsActive;
        p.IsAmazingOffer = input.IsAmazingOffer;
        p.OfferEndsAt = input.IsAmazingOffer && input.OfferEndsAt != null
            ? DateTime.SpecifyKind(input.OfferEndsAt.Value.ToUniversalTime(), DateTimeKind.Utc)
            : null;

        var baseSlug = Slug.Generate(string.IsNullOrWhiteSpace(input.Slug) ? input.Title : input.Slug);
        if (p.Slug != baseSlug)
            p.Slug = await Slug.UniqueAsync(baseSlug, s => db.Products.AnyAsync(x => x.Slug == s && x.Id != p.Id, ct));

        // Sync variants: update existing, add new, remove missing.
        var keepIds = input.Variants.Where(v => v.Id is > 0).Select(v => v.Id!.Value).ToHashSet();
        foreach (var removed in p.Variants.Where(v => !keepIds.Contains(v.Id)).ToList())
        {
            p.Variants.Remove(removed);
            db.ProductVariants.Remove(removed);
        }
        foreach (var v in input.Variants)
        {
            var variant = v.Id is > 0 ? p.Variants.FirstOrDefault(x => x.Id == v.Id) : null;
            if (variant == null)
            {
                variant = new ProductVariant();
                p.Variants.Add(variant);
            }
            variant.ColorName = string.IsNullOrWhiteSpace(v.ColorName) ? null : v.ColorName.Trim();
            variant.ColorHex = string.IsNullOrWhiteSpace(v.ColorHex) ? null : v.ColorHex.Trim().ToLowerInvariant();
            variant.Sku = string.IsNullOrWhiteSpace(v.Sku) ? null : v.Sku.Trim();
            variant.Price = v.Price;
            variant.DiscountPercent = v.DiscountPercent;
            variant.Stock = v.Stock;
        }

        // Specifications are simply replaced.
        db.ProductSpecifications.RemoveRange(p.Specifications);
        p.Specifications.Clear();
        var order = 0;
        foreach (var s in input.Specifications.Where(s => !string.IsNullOrWhiteSpace(s.Key)))
            p.Specifications.Add(new ProductSpecification { Key = s.Key.Trim(), Value = s.Value.Trim(), DisplayOrder = order++ });

        p.RecalculateSummary();
    }

    private static AdminProductDto ToDto(Product p) => new(
        p.Id, p.Title, p.EnglishTitle, p.Slug, p.Description, p.CategoryId, p.BrandId, p.IsActive, p.IsAmazingOffer, p.OfferEndsAt,
        p.Images.OrderBy(i => i.DisplayOrder).Select(i => new AdminImageDto(i.Id, i.Url, i.DisplayOrder)).ToList(),
        p.Variants.OrderBy(v => v.Id).Select(v => new AdminVariantDto(v.Id, v.ColorName, v.ColorHex, v.Sku, v.Price, v.DiscountPercent, v.Stock)).ToList(),
        p.Specifications.OrderBy(s => s.DisplayOrder).Select(s => new SpecificationDto(s.Key, s.Value)).ToList());
}

// ---------------- Categories ----------------

public class AdminCategoryService(IAppDbContext db, IFileStorage storage)
{
    public async Task<List<AdminCategoryDto>> ListAsync(CancellationToken ct) =>
        await db.Categories.AsNoTracking().OrderBy(c => c.ParentId ?? 0).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id)
            .Select(c => new AdminCategoryDto(c.Id, c.Name, c.Slug, c.Icon, c.ImageUrl, c.ParentId,
                c.Parent != null ? c.Parent.Name : null, c.DisplayOrder, c.IsActive, c.Products.Count))
            .ToListAsync(ct);

    public async Task<int> CreateAsync(CategoryInput input, CancellationToken ct)
    {
        var category = new Category();
        await ApplyAsync(category, input, ct);
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category.Id;
    }

    public async Task UpdateAsync(int id, CategoryInput input, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw new NotFoundException("دسته‌بندی پیدا نشد.");
        if (input.ParentId == id) throw new AppException("یک دسته‌بندی نمی‌تواند والد خودش باشد.");
        if (input.ParentId != null)
        {
            var tree = await CategoryTree.LoadAsync(db, false, ct);
            if (tree.DescendantIdsIncludingSelf(id).Contains(input.ParentId.Value))
                throw new AppException("نمی‌توان یک زیرمجموعه را به عنوان والد انتخاب کرد.");
        }
        await ApplyAsync(category, input, ct);
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw new NotFoundException("دسته‌بندی پیدا نشد.");
        if (await db.Categories.AnyAsync(c => c.ParentId == id, ct)) throw new AppException("ابتدا زیرمجموعه‌های این دسته را حذف یا جابه‌جا کنید.");
        if (await db.Products.AnyAsync(p => p.CategoryId == id, ct)) throw new AppException("این دسته‌بندی محصول دارد و قابل حذف نیست.");
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        storage.Delete(category.ImageUrl);
    }

    public async Task<string> SetImageAsync(int id, FileUpload file, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct) ?? throw new NotFoundException("دسته‌بندی پیدا نشد.");
        var old = category.ImageUrl;
        category.ImageUrl = await storage.SaveAsync(file, "categories", ct);
        await db.SaveChangesAsync(ct);
        storage.Delete(old);
        return category.ImageUrl;
    }

    private async Task ApplyAsync(Category c, CategoryInput input, CancellationToken ct)
    {
        if (input.ParentId != null && !await db.Categories.AnyAsync(x => x.Id == input.ParentId, ct))
            throw new AppException("دسته‌بندی والد وجود ندارد.");
        c.Name = input.Name.Trim();
        c.Icon = string.IsNullOrWhiteSpace(input.Icon) ? null : input.Icon.Trim();
        c.ParentId = input.ParentId;
        c.DisplayOrder = input.DisplayOrder;
        c.IsActive = input.IsActive;
        var baseSlug = Slug.Generate(string.IsNullOrWhiteSpace(input.Slug) ? input.Name : input.Slug);
        if (c.Slug != baseSlug)
            c.Slug = await Slug.UniqueAsync(baseSlug, s => db.Categories.AnyAsync(x => x.Slug == s && x.Id != c.Id, ct));
    }
}

// ---------------- Brands ----------------

public class AdminBrandService(IAppDbContext db, IFileStorage storage)
{
    public async Task<List<AdminBrandDto>> ListAsync(CancellationToken ct) =>
        await db.Brands.AsNoTracking().OrderBy(b => b.Name)
            .Select(b => new AdminBrandDto(b.Id, b.Name, b.EnglishName, b.Slug, b.LogoUrl, b.Products.Count)).ToListAsync(ct);

    public async Task<int> CreateAsync(BrandInput input, CancellationToken ct)
    {
        var brand = new Brand();
        await ApplyAsync(brand, input, ct);
        db.Brands.Add(brand);
        await db.SaveChangesAsync(ct);
        return brand.Id;
    }

    public async Task UpdateAsync(int id, BrandInput input, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException("برند پیدا نشد.");
        await ApplyAsync(brand, input, ct);
        brand.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException("برند پیدا نشد.");
        if (await db.Products.AnyAsync(p => p.BrandId == id, ct)) throw new AppException("این برند محصول دارد و قابل حذف نیست.");
        db.Brands.Remove(brand);
        await db.SaveChangesAsync(ct);
        storage.Delete(brand.LogoUrl);
    }

    public async Task<string> SetLogoAsync(int id, FileUpload file, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException("برند پیدا نشد.");
        var old = brand.LogoUrl;
        brand.LogoUrl = await storage.SaveAsync(file, "brands", ct);
        await db.SaveChangesAsync(ct);
        storage.Delete(old);
        return brand.LogoUrl;
    }

    private async Task ApplyAsync(Brand b, BrandInput input, CancellationToken ct)
    {
        b.Name = input.Name.Trim();
        b.EnglishName = input.EnglishName.Trim();
        var baseSlug = Slug.Generate(string.IsNullOrWhiteSpace(input.Slug) ? input.EnglishName : input.Slug);
        if (b.Slug != baseSlug)
            b.Slug = await Slug.UniqueAsync(baseSlug, s => db.Brands.AnyAsync(x => x.Slug == s && x.Id != b.Id, ct));
    }
}
