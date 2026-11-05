namespace Bazaarche.Domain.Enums;

public enum UserRole { Customer = 0, Admin = 1 }

public enum OrderStatus
{
    PendingPayment = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public enum PaymentStatus { Pending = 0, Succeeded = 1, Failed = 2 }

public enum ShippingMethod { Standard = 0, Express = 1 }

public enum CouponType { Percent = 0, Fixed = 1 }

public enum BannerPosition { HeroSlider = 0, MiddleBanner = 1 }
