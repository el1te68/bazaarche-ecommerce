using System.Security.Cryptography;
using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Orders;

public class CheckoutService(IAppDbContext db, ICurrentUser currentUser)
{
    public const long FreeShippingThreshold = 1_500_000;
    public const long StandardShippingCost = 49_000;
    public const long ExpressShippingCost = 89_000;

    public static List<ShippingOptionDto> ShippingOptions(long payableItems) =>
    [
        new(ShippingMethod.Standard, "ارسال عادی", "تحویل ۲ تا ۴ روز کاری",
            payableItems >= FreeShippingThreshold ? 0 : StandardShippingCost),
        new(ShippingMethod.Express, "ارسال اکسپرس", "تحویل فردا (فقط تهران و کرج)", ExpressShippingCost)
    ];

    private record CartSnapshot(List<(CartItem Item, ProductVariant Variant)> Lines, long ItemsTotal, long Payable);

    private async Task<CartSnapshot> LoadCartAsync(int userId, bool tracking, CancellationToken ct)
    {
        var query = db.CartItems.Where(c => c.UserId == userId)
            .Include(c => c.ProductVariant!).ThenInclude(v => v.Product!).ThenInclude(p => p.Images)
            .AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        var items = await query.OrderBy(c => c.Id).ToListAsync(ct);

        var lines = items.Where(i => i.ProductVariant?.Product is { IsActive: true })
            .Select(i => (Item: i, Variant: i.ProductVariant!)).ToList();
        var itemsTotal = lines.Sum(l => l.Variant.Price * l.Item.Quantity);
        var payable = lines.Sum(l => l.Variant.GetFinalPrice() * l.Item.Quantity);
        return new CartSnapshot(lines, itemsTotal, payable);
    }

    private async Task<(Coupon? coupon, long discount, string? message)> EvaluateCouponAsync(string? code, long payableItems, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return (null, 0, null);
        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == normalized, ct);
        if (coupon == null || !coupon.IsActive) return (null, 0, "کد تخفیف معتبر نیست.");
        if (coupon.ExpiresAt != null && coupon.ExpiresAt < DateTime.UtcNow) return (null, 0, "مهلت استفاده از این کد تخفیف تمام شده است.");
        if (coupon.UsageLimit != null && coupon.UsedCount >= coupon.UsageLimit) return (null, 0, "ظرفیت استفاده از این کد تخفیف تکمیل شده است.");
        if (payableItems < coupon.MinOrderAmount)
            return (null, 0, $"این کد برای خریدهای بالای {coupon.MinOrderAmount:N0} تومان قابل استفاده است.");
        var discount = coupon.CalculateDiscount(payableItems);
        return (coupon, discount, $"کد تخفیف اعمال شد: {discount:N0} تومان تخفیف");
    }

    public async Task<CheckoutPreviewDto> PreviewAsync(CheckoutPreviewRequest request, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var cart = await LoadCartAsync(userId, false, ct);
        var (coupon, couponDiscount, message) = await EvaluateCouponAsync(request.CouponCode, cart.Payable, ct);
        var options = ShippingOptions(cart.Payable);
        var shipping = options.First(o => o.Method == request.ShippingMethod).Cost;
        return new CheckoutPreviewDto(cart.ItemsTotal, cart.ItemsTotal - cart.Payable, shipping, couponDiscount,
            cart.Payable - couponDiscount + shipping, message, coupon != null, options);
    }

    public async Task<CheckoutResultDto> CheckoutAsync(CheckoutRequest request, CancellationToken ct)
    {
        var userId = currentUser.RequireId();
        var address = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId, ct)
                      ?? throw new AppException("آدرس انتخاب‌شده معتبر نیست.");
        var cart = await LoadCartAsync(userId, true, ct);
        if (cart.Lines.Count == 0) throw new AppException("سبد خرید شما خالی است.");

        foreach (var (item, variant) in cart.Lines)
        {
            if (variant.Stock < item.Quantity)
                throw new AppException(variant.Stock == 0
                    ? $"«{variant.Product!.Title}» ناموجود شده است. لطفاً آن را از سبد خرید حذف کنید."
                    : $"از «{variant.Product!.Title}» فقط {variant.Stock} عدد موجود است.");
        }

        var (coupon, couponDiscount, couponMessage) = await EvaluateCouponAsync(request.CouponCode, cart.Payable, ct);
        if (!string.IsNullOrWhiteSpace(request.CouponCode) && coupon == null) throw new AppException(couponMessage ?? "کد تخفیف معتبر نیست.");
        var shipping = ShippingOptions(cart.Payable).First(o => o.Method == request.ShippingMethod).Cost;

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(ct),
            UserId = userId,
            Status = OrderStatus.PendingPayment,
            ReceiverName = address.ReceiverName,
            ReceiverPhone = address.ReceiverPhone,
            Province = address.Province,
            City = address.City,
            FullAddress = address.FullAddress,
            PostalCode = address.PostalCode,
            ShippingMethod = request.ShippingMethod,
            ShippingCost = shipping,
            ItemsTotal = cart.ItemsTotal,
            ProductsDiscount = cart.ItemsTotal - cart.Payable,
            CouponCode = coupon?.Code,
            CouponDiscount = couponDiscount,
            PayableAmount = cart.Payable - couponDiscount + shipping
        };

        foreach (var (item, variant) in cart.Lines)
        {
            var product = variant.Product!;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant.Id,
                ProductTitle = product.Title,
                ProductSlug = product.Slug,
                ImageUrl = product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                ColorName = variant.ColorName,
                UnitPrice = variant.Price,
                UnitFinalPrice = variant.GetFinalPrice(),
                Quantity = item.Quantity
            });
            // Reserve stock until the payment succeeds or the order is cancelled.
            variant.Stock -= item.Quantity;
        }

        var payment = NewPayment(order.PayableAmount);
        order.Payments.Add(payment);
        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.Lines.Select(l => l.Item));
        await db.SaveChangesAsync(ct);

        await RecalculateProductsAsync(db, order.Items.Select(i => i.ProductId), ct);
        await db.SaveChangesAsync(ct);

        return new CheckoutResultDto(order.Id, order.OrderNumber, payment.Token, order.PayableAmount);
    }

    public static Payment NewPayment(long amount) => new()
    {
        Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
        Amount = amount,
        Status = PaymentStatus.Pending
    };

    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        while (true)
        {
            var number = "BZ-" + RandomNumberGenerator.GetInt32(10_000_000, 99_999_999);
            if (!await db.Orders.AnyAsync(o => o.OrderNumber == number, ct)) return number;
        }
    }

    /// <summary>Re-syncs the denormalized price/stock summary of the given products.</summary>
    public static async Task RecalculateProductsAsync(IAppDbContext db, IEnumerable<int> productIds, CancellationToken ct)
    {
        var ids = productIds.Distinct().ToList();
        var products = await db.Products.Include(p => p.Variants).Where(p => ids.Contains(p.Id)).ToListAsync(ct);
        foreach (var p in products) p.RecalculateSummary();
    }

    /// <summary>Puts the reserved stock of an order back (used when an order is cancelled).</summary>
    public static async Task RestockAsync(IAppDbContext db, Order order, CancellationToken ct)
    {
        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
        var variants = await db.ProductVariants.Where(v => variantIds.Contains(v.Id)).ToListAsync(ct);
        foreach (var item in order.Items)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
            if (variant != null) variant.Stock += item.Quantity;
        }
        await db.SaveChangesAsync(ct);
        await RecalculateProductsAsync(db, order.Items.Select(i => i.ProductId), ct);
    }
}
