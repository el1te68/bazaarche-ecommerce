using Bazaarche.Application.Features.Admin;
using Bazaarche.Application.Features.Auth;
using Bazaarche.Application.Features.Catalog;
using Bazaarche.Application.Features.Orders;
using Bazaarche.Application.Features.Shopping;
using Microsoft.Extensions.DependencyInjection;

namespace Bazaarche.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<CartService>();
        services.AddScoped<WishlistService>();
        services.AddScoped<AddressService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<CheckoutService>();
        services.AddScoped<OrderService>();
        services.AddScoped<PaymentService>();

        services.AddScoped<AdminDashboardService>();
        services.AddScoped<AdminProductService>();
        services.AddScoped<AdminCategoryService>();
        services.AddScoped<AdminBrandService>();
        services.AddScoped<AdminOrderService>();
        services.AddScoped<AdminCouponService>();
        services.AddScoped<AdminUserService>();
        services.AddScoped<AdminReviewService>();
        services.AddScoped<AdminBannerService>();
        return services;
    }
}
