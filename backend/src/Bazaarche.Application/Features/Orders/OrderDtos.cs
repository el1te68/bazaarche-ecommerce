using System.ComponentModel.DataAnnotations;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;

namespace Bazaarche.Application.Features.Orders;

public class CheckoutPreviewRequest
{
    public ShippingMethod ShippingMethod { get; init; } = ShippingMethod.Standard;
    public string? CouponCode { get; init; }
}

public class CheckoutRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "آدرس تحویل را انتخاب کنید.")]
    public int AddressId { get; init; }
    public ShippingMethod ShippingMethod { get; init; } = ShippingMethod.Standard;
    public string? CouponCode { get; init; }
}

public record ShippingOptionDto(ShippingMethod Method, string Title, string Description, long Cost);

public record CheckoutPreviewDto(
    long ItemsTotal,
    long ProductsDiscount,
    long ShippingCost,
    long CouponDiscount,
    long Payable,
    string? CouponMessage,
    bool CouponApplied,
    List<ShippingOptionDto> ShippingOptions);

public record CheckoutResultDto(int OrderId, string OrderNumber, string PaymentToken, long Amount);

public record OrderItemDto(int ProductId, int VariantId, string Title, string Slug, string? ImageUrl, string? ColorName,
    long UnitPrice, long UnitFinalPrice, int Quantity);

public record OrderSummaryDto(int Id, string OrderNumber, OrderStatus Status, DateTime CreatedAt, long PayableAmount,
    int ItemsCount, List<string?> Images);

public record PaymentDto(int Id, string Token, long Amount, PaymentStatus Status, string? ReferenceId, string? CardMask,
    DateTime CreatedAt, DateTime? CompletedAt);

public record OrderDetailDto(
    int Id,
    string OrderNumber,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    string ReceiverName,
    string ReceiverPhone,
    string Province,
    string City,
    string FullAddress,
    string PostalCode,
    ShippingMethod ShippingMethod,
    long ShippingCost,
    long ItemsTotal,
    long ProductsDiscount,
    string? CouponCode,
    long CouponDiscount,
    long PayableAmount,
    string? TrackingCode,
    List<OrderItemDto> Items,
    List<PaymentDto> Payments,
    string? CustomerName = null,
    string? CustomerPhone = null);

public record GatewayInfoDto(string Token, string OrderNumber, long Amount, string MerchantName, PaymentStatus Status, DateTime CreatedAt);

public class CompletePaymentRequest
{
    public bool Success { get; init; }
    public string? CardNumber { get; init; }
}

public record PaymentResultDto(bool Success, int OrderId, string OrderNumber, long Amount, string? ReferenceId, string Message);

public static class OrderMapping
{
    public static OrderDetailDto ToDetail(this Order o, bool includeCustomer = false) => new(
        o.Id, o.OrderNumber, o.Status, o.CreatedAt, o.PaidAt,
        o.ReceiverName, o.ReceiverPhone, o.Province, o.City, o.FullAddress, o.PostalCode,
        o.ShippingMethod, o.ShippingCost, o.ItemsTotal, o.ProductsDiscount, o.CouponCode, o.CouponDiscount,
        o.PayableAmount, o.TrackingCode,
        o.Items.OrderBy(i => i.Id).Select(i => new OrderItemDto(i.ProductId, i.ProductVariantId, i.ProductTitle, i.ProductSlug,
            i.ImageUrl, i.ColorName, i.UnitPrice, i.UnitFinalPrice, i.Quantity)).ToList(),
        o.Payments.OrderByDescending(p => p.Id).Select(p => new PaymentDto(p.Id, p.Token, p.Amount, p.Status, p.ReferenceId,
            p.CardMask, p.CreatedAt, p.CompletedAt)).ToList(),
        includeCustomer ? o.User?.FullName : null,
        includeCustomer ? o.User?.PhoneNumber : null);
}
