using Bazaarche.Application.Features.Auth;
using Bazaarche.Application.Features.Catalog;
using Bazaarche.Application.Features.Orders;
using Bazaarche.Application.Features.Shopping;
using Bazaarche.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bazaarche.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register"), EnableRateLimiting("auth")]
    public Task<AuthResponse> Register(RegisterRequest request, CancellationToken ct) => auth.RegisterAsync(request, ct);

    [HttpPost("login"), EnableRateLimiting("auth")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken ct) => auth.LoginAsync(request, ct);

    [HttpGet("me"), Authorize]
    public Task<UserDto> Me(CancellationToken ct) => auth.MeAsync(ct);

    [HttpPut("profile"), Authorize]
    public Task<UserDto> UpdateProfile(UpdateProfileRequest request, CancellationToken ct) => auth.UpdateProfileAsync(request, ct);

    [HttpPut("password"), Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(request, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api")]
public class CatalogController(CatalogService catalog, ReviewService reviews) : ControllerBase
{
    [HttpGet("home")]
    public Task<HomeDto> Home(CancellationToken ct) => catalog.GetHomeAsync(ct);

    [HttpGet("categories")]
    public Task<List<CategoryNodeDto>> Categories(CancellationToken ct) => catalog.GetCategoryTreeAsync(ct);

    [HttpGet("brands")]
    public Task<List<BrandDto>> Brands(CancellationToken ct) => catalog.GetBrandsAsync(ct);

    [HttpGet("products")]
    public Task<ProductListResponse> Products([FromQuery] ProductQuery query, CancellationToken ct) => catalog.SearchAsync(query, ct);

    [HttpGet("products/suggest")]
    public Task<SuggestDto> Suggest([FromQuery] string? q, CancellationToken ct) => catalog.SuggestAsync(q, ct);

    [HttpGet("products/{slug}")]
    public Task<ProductDetailDto> Product(string slug, CancellationToken ct) => catalog.GetProductAsync(slug, ct);

    [HttpGet("products/{id:int}/related")]
    public Task<List<ProductCardDto>> Related(int id, CancellationToken ct) => catalog.GetRelatedAsync(id, ct);

    [HttpGet("products/{id:int}/reviews")]
    public Task<ReviewSummaryDto> Reviews(int id, [FromQuery] int page, CancellationToken ct) => reviews.GetForProductAsync(id, page, ct);

    [HttpPost("products/{id:int}/reviews"), Authorize]
    public async Task<IActionResult> AddReview(int id, CreateReviewRequest request, CancellationToken ct)
    {
        await reviews.CreateAsync(id, request, ct);
        return Ok(new { message = "نظر شما ثبت شد و پس از تایید نمایش داده می‌شود." });
    }
}

[ApiController]
[Route("api/wishlist"), Authorize]
public class WishlistController(WishlistService wishlist) : ControllerBase
{
    [HttpGet]
    public Task<List<ProductCardDto>> Get(CancellationToken ct) => wishlist.GetAsync(ct);

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId, CancellationToken ct)
    {
        await wishlist.AddAsync(productId, ct);
        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct)
    {
        await wishlist.RemoveAsync(productId, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/cart")]
public class CartController(CartService cart) : ControllerBase
{
    /// <summary>Prices a guest cart stored in the browser.</summary>
    [HttpPost("preview")]
    public Task<CartDto> Preview(List<CartLine> lines, CancellationToken ct) => cart.PreviewAsync(lines, ct);

    [HttpGet, Authorize]
    public Task<CartDto> Get(CancellationToken ct) => cart.GetAsync(ct);

    [HttpPost("items"), Authorize]
    public Task<CartDto> Add(AddToCartRequest request, CancellationToken ct) => cart.AddAsync(request, ct);

    [HttpPut("items/{variantId:int}"), Authorize]
    public Task<CartDto> Update(int variantId, UpdateCartItemRequest request, CancellationToken ct) => cart.UpdateAsync(variantId, request, ct);

    [HttpDelete("items/{variantId:int}"), Authorize]
    public Task<CartDto> Remove(int variantId, CancellationToken ct) => cart.RemoveAsync(variantId, ct);

    [HttpDelete, Authorize]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await cart.ClearAsync(ct);
        return NoContent();
    }

    [HttpPost("merge"), Authorize]
    public Task<CartDto> Merge(List<CartLine> lines, CancellationToken ct) => cart.MergeAsync(lines, ct);
}

[ApiController]
[Route("api/addresses"), Authorize]
public class AddressesController(AddressService addresses) : ControllerBase
{
    [HttpGet]
    public Task<List<AddressDto>> GetAll(CancellationToken ct) => addresses.GetAllAsync(ct);

    [HttpPost]
    public Task<AddressDto> Create(AddressRequest request, CancellationToken ct) => addresses.CreateAsync(request, ct);

    [HttpPut("{id:int}")]
    public Task<AddressDto> Update(int id, AddressRequest request, CancellationToken ct) => addresses.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await addresses.DeleteAsync(id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/checkout"), Authorize]
public class CheckoutController(CheckoutService checkout) : ControllerBase
{
    [HttpPost("preview")]
    public Task<CheckoutPreviewDto> Preview(CheckoutPreviewRequest request, CancellationToken ct) => checkout.PreviewAsync(request, ct);

    [HttpPost]
    public Task<CheckoutResultDto> Checkout(CheckoutRequest request, CancellationToken ct) => checkout.CheckoutAsync(request, ct);
}

[ApiController]
[Route("api/orders"), Authorize]
public class OrdersController(OrderService orders) : ControllerBase
{
    [HttpGet]
    public Task<List<OrderSummaryDto>> GetAll([FromQuery] OrderStatus? status, CancellationToken ct) => orders.GetMyOrdersAsync(status, ct);

    [HttpGet("{id:int}")]
    public Task<OrderDetailDto> Get(int id, CancellationToken ct) => orders.GetMyOrderAsync(id, ct);

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await orders.CancelAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/pay")]
    public Task<CheckoutResultDto> Pay(int id, CancellationToken ct) => orders.RetryPaymentAsync(id, ct);
}

/// <summary>Endpoints of the simulated bank gateway (public, like a real gateway page).</summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentService payments) : ControllerBase
{
    [HttpGet("{token}")]
    public Task<GatewayInfoDto> Get(string token, CancellationToken ct) => payments.GetAsync(token, ct);

    [HttpPost("{token}/complete")]
    public Task<PaymentResultDto> Complete(string token, CompletePaymentRequest request, CancellationToken ct) =>
        payments.CompleteAsync(token, request, ct);
}
