using Bazaarche.Domain.Common;
using Bazaarche.Domain.Enums;

namespace Bazaarche.Domain.Entities;

public class CartItem : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
}

public class WishlistItem : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    /// <summary>Percent (1-100) or fixed amount in Toman, depending on <see cref="Type"/>.</summary>
    public long Value { get; set; }
    public long? MaxDiscountAmount { get; set; }
    public long MinOrderAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;

    public long CalculateDiscount(long amount)
    {
        long discount = Type == CouponType.Percent ? amount * Value / 100 : Value;
        if (MaxDiscountAmount is > 0 && discount > MaxDiscountAmount) discount = MaxDiscountAmount.Value;
        return Math.Min(discount, amount);
    }
}

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    // Shipping address snapshot
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public ShippingMethod ShippingMethod { get; set; }
    public long ShippingCost { get; set; }
    public long ItemsTotal { get; set; }
    public long ProductsDiscount { get; set; }
    public string? CouponCode { get; set; }
    public long CouponDiscount { get; set; }
    public long PayableAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TrackingCode { get; set; }

    public List<OrderItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ColorName { get; set; }
    public long UnitPrice { get; set; }
    public long UnitFinalPrice { get; set; }
    public int Quantity { get; set; }
}

public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public string Token { get; set; } = string.Empty;
    public long Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ReferenceId { get; set; }
    public string? CardMask { get; set; }
    public DateTime? CompletedAt { get; set; }
}
