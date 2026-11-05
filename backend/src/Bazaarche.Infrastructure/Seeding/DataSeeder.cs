using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Bazaarche.Infrastructure.Persistence;
using Bazaarche.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bazaarche.Infrastructure.Seeding;

public class SeedOptions
{
    public const string Section = "Seed";
    public string AdminPhone { get; set; } = "09120000000";
    public string AdminPassword { get; set; } = "Admin@123";
    public string AdminName { get; set; } = "مدیر سایت";
    /// <summary>Seeds the demo catalog, customers, reviews and orders when the database is empty.</summary>
    public bool DemoData { get; set; } = true;
}

public class DataSeeder(
    AppDbContext db,
    IPasswordHasher hasher,
    LocalFileStorage storage,
    IOptions<SeedOptions> options,
    ILogger<DataSeeder> logger)
{
    private readonly Random _random = new(1405);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureAdminAsync(ct);
        if (!options.Value.DemoData || await db.Categories.AnyAsync(ct)) return;

        logger.LogInformation("Seeding demo data...");
        var categories = await SeedCategoriesAsync(ct);
        var brands = await SeedBrandsAsync(ct);
        var products = await SeedProductsAsync(categories, brands, ct);
        await SeedBannersAsync(ct);
        await SeedCouponsAsync(ct);
        var customers = await SeedCustomersAsync(ct);
        await SeedReviewsAsync(products, customers, ct);
        await SeedOrdersAsync(products, customers, ct);
        logger.LogInformation("Demo data seeded: {Products} products, {Customers} customers.", products.Count, customers.Count);
    }

    private async Task EnsureAdminAsync(CancellationToken ct)
    {
        var o = options.Value;
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin, ct)) return;
        db.Users.Add(new User
        {
            FullName = o.AdminName,
            PhoneNumber = o.AdminPhone,
            Email = "admin@bazaarche.local",
            PasswordHash = hasher.Hash(o.AdminPassword),
            Role = UserRole.Admin
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Admin user created: {Phone}", o.AdminPhone);
    }

    private string WriteSvg(string folder, string name, string svg)
    {
        var dir = Path.Combine(storage.Root, "seed", folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, name), svg);
        return $"/uploads/seed/{folder}/{name}";
    }

    private async Task<Dictionary<string, Category>> SeedCategoriesAsync(CancellationToken ct)
    {
        var map = new Dictionary<string, Category>();
        var order = 0;
        foreach (var root in SeedCatalog.Categories)
        {
            var parent = new Category { Name = root.Name, Slug = root.Slug, Icon = root.Icon, DisplayOrder = order++ };
            map[root.Slug] = parent;
            var childOrder = 0;
            foreach (var child in root.Children ?? [])
            {
                var c = new Category { Name = child.Name, Slug = child.Slug, Icon = child.Icon, DisplayOrder = childOrder++, Parent = parent };
                parent.Children.Add(c);
                map[child.Slug] = c;
            }
            db.Categories.Add(parent);
        }
        await db.SaveChangesAsync(ct);
        return map;
    }

    private async Task<Dictionary<string, Brand>> SeedBrandsAsync(CancellationToken ct)
    {
        var map = new Dictionary<string, Brand>();
        foreach (var b in SeedCatalog.Brands)
        {
            var brand = new Brand
            {
                Name = b.Name,
                EnglishName = b.EnglishName,
                Slug = b.Slug,
                LogoUrl = WriteSvg("brands", $"{b.Slug}.svg", SvgArt.BrandLogo(b.EnglishName, b.Color))
            };
            map[b.Slug] = brand;
            db.Brands.Add(brand);
        }
        await db.SaveChangesAsync(ct);
        return map;
    }

    private async Task<List<Product>> SeedProductsAsync(Dictionary<string, Category> categories, Dictionary<string, Brand> brands, CancellationToken ct)
    {
        var list = new List<Product>();
        var index = 0;
        var now = DateTime.UtcNow;
        foreach (var s in SeedCatalog.Products)
        {
            index++;
            var brand = s.Brand != null ? brands[s.Brand] : null;
            var product = new Product
            {
                Title = s.Title,
                EnglishTitle = s.EnglishTitle,
                Slug = Slug.Generate(s.EnglishTitle),
                Description = SeedCatalog.Description(s, brand?.Name),
                Category = categories[s.Category],
                Brand = brand,
                IsAmazingOffer = s.Amazing && s.Discount > 0,
                OfferEndsAt = s.Amazing ? now.Date.AddDays(1 + index % 3).AddHours(20.5) : null,
                ViewCount = _random.Next(50, 5000),
                // Spread creation dates so "newest" sorting looks natural.
                CreatedAt = now.AddDays(-(SeedCatalog.Products.Length - index)).AddMinutes(-_random.Next(0, 600))
            };

            // Variants: one per color (prices differ slightly per color, like real shops).
            var colors = s.Colors.Length == 0
                ? new SeedCatalog.ColorSeed?[] { null }
                : s.Colors.Select(c => (SeedCatalog.ColorSeed?)c).ToArray();
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var price = RoundPrice(s.Price * (1 + i * 0.015));
                var stock = s.Stock == 0 ? 0 : Math.Max(i == colors.Length - 1 && colors.Length > 2 ? 0 : 1, s.Stock - i * 3);
                product.Variants.Add(new ProductVariant
                {
                    ColorName = color?.Name,
                    ColorHex = color?.Hex,
                    Sku = $"BZ-{index:000}-{i + 1}",
                    Price = price,
                    DiscountPercent = i == 0 ? s.Discount : Math.Max(0, s.Discount - 2 * i),
                    Stock = stock
                });
            }

            // Images: main image + up to two extra shots in other colors/angles.
            var baseColor = s.Colors.FirstOrDefault()?.Hex ?? SeedCatalog.KindColors.GetValueOrDefault(s.Kind, "#2563eb");
            var label = s.Label ?? brand?.EnglishName ?? "Bazaarche";
            var shots = new List<(string Color, int Variant)> { (baseColor, 0) };
            shots.Add((s.Colors.Length > 1 ? s.Colors[1].Hex : baseColor, 1));
            if (s.Colors.Length > 2) shots.Add((s.Colors[2].Hex, 2)); else shots.Add((baseColor, 2));
            for (var i = 0; i < shots.Count; i++)
            {
                var url = WriteSvg("products", $"p{index:000}-{i + 1}.svg", SvgArt.Product(s.Kind, shots[i].Color, shots[i].Variant, label));
                product.Images.Add(new ProductImage { Url = url, DisplayOrder = i });
            }

            var specOrder = 0;
            foreach (var pair in s.Specs.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split(':', 2);
                if (kv.Length == 2)
                    product.Specifications.Add(new ProductSpecification { Key = kv[0].Trim(), Value = kv[1].Trim(), DisplayOrder = specOrder++ });
            }
            if (brand != null)
                product.Specifications.Insert(0, new ProductSpecification { Key = "برند", Value = brand.Name, DisplayOrder = -1 });

            product.RecalculateSummary();
            list.Add(product);
            db.Products.Add(product);
        }
        await db.SaveChangesAsync(ct);
        return list;
    }

    private static long RoundPrice(double price) => (long)(Math.Round(price / 10_000) * 10_000);

    private async Task SeedBannersAsync(CancellationToken ct)
    {
        var banners = new List<Banner>
        {
            new() { Title = "جشنواره پاییزه بازارچه", Position = BannerPosition.HeroSlider, DisplayOrder = 0, LinkUrl = "/search?amazing=true",
                ImageUrl = WriteSvg("banners", "hero-1.svg", SvgArt.Banner("جشنواره پاییزه بازارچه", "تا ۴۰٪ تخفیف روی هزاران کالا", "#ef4056", "#a3122d", "مشاهده تخفیف‌ها")) },
            new() { Title = "گوشی‌های جدید", Position = BannerPosition.HeroSlider, DisplayOrder = 1, LinkUrl = "/category/mobile-phone",
                ImageUrl = WriteSvg("banners", "hero-2.svg", SvgArt.Banner("نسل جدید گوشی‌های هوشمند", "با اقساط بدون پیش‌پرداخت", "#4f46e5", "#1e1b4b", "خرید گوشی")) },
            new() { Title = "لپ‌تاپ برای همه", Position = BannerPosition.HeroSlider, DisplayOrder = 2, LinkUrl = "/category/laptop",
                ImageUrl = WriteSvg("banners", "hero-3.svg", SvgArt.Banner("لپ‌تاپ برای کار و سرگرمی", "ارسال رایگان برای خریدهای بالای ۱.۵ میلیون", "#0ea5e9", "#0c4a6e", "مشاهده لپ‌تاپ‌ها")) },
            new() { Title = "خانه هوشمند", Position = BannerPosition.HeroSlider, DisplayOrder = 3, LinkUrl = "/category/home-appliances",
                ImageUrl = WriteSvg("banners", "hero-4.svg", SvgArt.Banner("خانه‌ات رو هوشمند کن", "لوازم خانگی با گارانتی معتبر", "#10b981", "#064e3b", "خرید لوازم خانگی")) },
            new() { Title = "هدفون", Position = BannerPosition.MiddleBanner, DisplayOrder = 0, LinkUrl = "/category/headphones",
                ImageUrl = WriteSvg("banners", "mid-1.svg", SvgArt.SmallBanner("دنیای صدا", "هدفون و اسپیکر با بهترین قیمت", "#f59e0b", "#b45309")) },
            new() { Title = "مد و پوشاک", Position = BannerPosition.MiddleBanner, DisplayOrder = 1, LinkUrl = "/category/fashion",
                ImageUrl = WriteSvg("banners", "mid-2.svg", SvgArt.SmallBanner("استایل پاییزی", "کالکشن جدید مد و پوشاک", "#ec4899", "#831843")) },
            new() { Title = "کتاب", Position = BannerPosition.MiddleBanner, DisplayOrder = 2, LinkUrl = "/category/books",
                ImageUrl = WriteSvg("banners", "mid-3.svg", SvgArt.SmallBanner("هفته کتاب", "تا ۱۵٪ تخفیف کتاب‌های پرفروش", "#14b8a6", "#134e4a")) },
            new() { Title = "ورزش", Position = BannerPosition.MiddleBanner, DisplayOrder = 3, LinkUrl = "/category/sports",
                ImageUrl = WriteSvg("banners", "mid-4.svg", SvgArt.SmallBanner("همیشه در حرکت", "تجهیزات ورزش و سفر", "#6366f1", "#312e81")) }
        };
        db.Banners.AddRange(banners);
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCouponsAsync(CancellationToken ct)
    {
        db.Coupons.AddRange(
            new Coupon { Code = "WELCOME", Type = CouponType.Percent, Value = 10, MaxDiscountAmount = 500_000, MinOrderAmount = 500_000 },
            new Coupon { Code = "BAZAAR50", Type = CouponType.Fixed, Value = 50_000, MinOrderAmount = 300_000, UsageLimit = 1000 },
            new Coupon { Code = "EXPIRED", Type = CouponType.Percent, Value = 20, ExpiresAt = DateTime.UtcNow.AddDays(-3) });
        await db.SaveChangesAsync(ct);
    }

    private async Task<List<User>> SeedCustomersAsync(CancellationToken ct)
    {
        var password = hasher.Hash("Demo@123");
        var users = new List<User>
        {
            new()
            {
                FullName = "کاربر نمونه", PhoneNumber = "09121111111", Email = "demo@bazaarche.local", PasswordHash = password,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                Addresses =
                [
                    new Address
                    {
                        Title = "خانه", ReceiverName = "کاربر نمونه", ReceiverPhone = "09121111111", Province = "تهران", City = "تهران",
                        FullAddress = "خیابان ولیعصر، بالاتر از میدان ونک، کوچه نمونه، پلاک ۱۲، واحد ۳", PostalCode = "1968912345", IsDefault = true
                    }
                ]
            }
        };
        for (var i = 0; i < SeedCatalog.CustomerNames.Length; i++)
        {
            var name = SeedCatalog.CustomerNames[i];
            users.Add(new User
            {
                FullName = name,
                PhoneNumber = $"0912200{i:0000}",
                PasswordHash = password,
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(5, 90)),
                Addresses =
                [
                    new Address
                    {
                        Title = "خانه", ReceiverName = name, ReceiverPhone = $"0912200{i:0000}",
                        Province = i % 3 == 0 ? "اصفهان" : i % 3 == 1 ? "تهران" : "خراسان رضوی",
                        City = i % 3 == 0 ? "اصفهان" : i % 3 == 1 ? "تهران" : "مشهد",
                        FullAddress = $"خیابان نمونه {i + 1}، کوچه گلستان، پلاک {10 + i}", PostalCode = $"81{_random.Next(10000000, 99999999)}",
                        IsDefault = true
                    }
                ]
            });
        }
        db.Users.AddRange(users);
        await db.SaveChangesAsync(ct);
        return users;
    }

    private async Task SeedReviewsAsync(List<Product> products, List<User> customers, CancellationToken ct)
    {
        var reviewers = customers.Skip(1).ToList();
        foreach (var product in products.Where((_, i) => i % 5 != 4))
        {
            var count = _random.Next(1, 5);
            foreach (var user in reviewers.OrderBy(_ => _random.Next()).Take(count))
            {
                var rating = _random.NextDouble() < 0.7 ? _random.Next(4, 6) : _random.Next(2, 4);
                db.Reviews.Add(new Review
                {
                    ProductId = product.Id,
                    UserId = user.Id,
                    Rating = rating,
                    Title = SeedCatalog.ReviewTitles[_random.Next(SeedCatalog.ReviewTitles.Length)],
                    Comment = SeedCatalog.ReviewComments[_random.Next(SeedCatalog.ReviewComments.Length)],
                    IsApproved = _random.NextDouble() > 0.08,
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 40))
                });
            }
        }
        await db.SaveChangesAsync(ct);

        foreach (var product in products)
        {
            var ratings = await db.Reviews.Where(r => r.ProductId == product.Id && r.IsApproved).Select(r => r.Rating).ToListAsync(ct);
            product.ReviewCount = ratings.Count;
            product.AverageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 1);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedOrdersAsync(List<Product> products, List<User> customers, CancellationToken ct)
    {
        var buyers = customers.ToList();
        var available = products.Where(p => p.TotalStock > 0).ToList();
        for (var n = 0; n < 45; n++)
        {
            var user = buyers[_random.Next(buyers.Count)];
            var address = user.Addresses.First();
            var daysAgo = _random.Next(0, 30);
            var created = DateTime.UtcNow.AddDays(-daysAgo).AddMinutes(-_random.Next(0, 600));
            var status = daysAgo switch
            {
                > 6 => _random.NextDouble() < 0.1 ? OrderStatus.Cancelled : OrderStatus.Delivered,
                > 2 => OrderStatus.Shipped,
                _ => _random.NextDouble() < 0.15 ? OrderStatus.PendingPayment : OrderStatus.Processing
            };

            var order = new Order
            {
                OrderNumber = $"BZ-{_random.Next(10_000_000, 99_999_999)}",
                UserId = user.Id,
                Status = status,
                CreatedAt = created,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone,
                Province = address.Province,
                City = address.City,
                FullAddress = address.FullAddress,
                PostalCode = address.PostalCode,
                ShippingMethod = _random.NextDouble() < 0.25 ? ShippingMethod.Express : ShippingMethod.Standard
            };

            foreach (var product in available.OrderBy(_ => _random.Next()).Take(_random.Next(1, 4)))
            {
                var variant = product.Variants.First();
                var qty = product.FinalPrice > 20_000_000 ? 1 : _random.Next(1, 3);
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductVariantId = variant.Id,
                    ProductTitle = product.Title,
                    ProductSlug = product.Slug,
                    ImageUrl = product.Images.OrderBy(i => i.DisplayOrder).First().Url,
                    ColorName = variant.ColorName,
                    UnitPrice = variant.Price,
                    UnitFinalPrice = variant.GetFinalPrice(),
                    Quantity = qty,
                    CreatedAt = created
                });
                if (status is not (OrderStatus.PendingPayment or OrderStatus.Cancelled)) product.SoldCount += qty;
            }

            order.ItemsTotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            var payable = order.Items.Sum(i => i.UnitFinalPrice * i.Quantity);
            order.ProductsDiscount = order.ItemsTotal - payable;
            order.ShippingCost = order.ShippingMethod == ShippingMethod.Express ? 89_000 : payable >= 1_500_000 ? 0 : 49_000;
            order.PayableAmount = payable + order.ShippingCost;

            var paid = status != OrderStatus.PendingPayment;
            if (paid) order.PaidAt = created.AddMinutes(3);
            if (status is OrderStatus.Shipped or OrderStatus.Delivered) order.TrackingCode = $"{_random.Next(100_000_000, 999_999_999)}";
            order.Payments.Add(new Payment
            {
                Token = Guid.NewGuid().ToString("N"),
                Amount = order.PayableAmount,
                Status = paid ? PaymentStatus.Succeeded : PaymentStatus.Pending,
                ReferenceId = paid ? _random.Next(100_000_000, 999_999_999).ToString() : null,
                CardMask = paid ? "603799******1234" : null,
                CreatedAt = created,
                CompletedAt = paid ? created.AddMinutes(3) : null
            });
            db.Orders.Add(order);
        }
        await db.SaveChangesAsync(ct);
    }
}
