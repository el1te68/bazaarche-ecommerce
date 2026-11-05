using Bazaarche.Api.Infrastructure;
using Bazaarche.Application.Common;
using Bazaarche.Application.Features.Admin;
using Bazaarche.Application.Features.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bazaarche.Api.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
public abstract class AdminControllerBase : ControllerBase;

[Route("api/admin/dashboard")]
public class AdminDashboardController(AdminDashboardService dashboard) : AdminControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get(CancellationToken ct) => dashboard.GetAsync(ct);
}

[Route("api/admin/products")]
public class AdminProductsController(AdminProductService products) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResult<AdminProductRowDto>> List([FromQuery] AdminProductQuery query, CancellationToken ct) => products.ListAsync(query, ct);

    [HttpGet("{id:int}")]
    public Task<AdminProductDto> Get(int id, CancellationToken ct) => products.GetAsync(id, ct);

    [HttpPost]
    public Task<AdminProductDto> Create(ProductInput input, CancellationToken ct) => products.CreateAsync(input, ct);

    [HttpPut("{id:int}")]
    public Task<AdminProductDto> Update(int id, ProductInput input, CancellationToken ct) => products.UpdateAsync(id, input, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await products.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/images"), RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<List<AdminImageDto>> UploadImages(int id, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        if (files.Count == 0) throw new AppException("هیچ فایلی انتخاب نشده است.");
        return await products.AddImagesAsync(id, files.Select(f => f.ToUpload()).ToList(), ct);
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public Task<List<AdminImageDto>> DeleteImage(int id, int imageId, CancellationToken ct) => products.DeleteImageAsync(id, imageId, ct);

    [HttpPut("{id:int}/images/order")]
    public Task<List<AdminImageDto>> ReorderImages(int id, List<int> imageIds, CancellationToken ct) => products.ReorderImagesAsync(id, imageIds, ct);
}

[Route("api/admin/categories")]
public class AdminCategoriesController(AdminCategoryService categories) : AdminControllerBase
{
    [HttpGet]
    public Task<List<AdminCategoryDto>> List(CancellationToken ct) => categories.ListAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Create(CategoryInput input, CancellationToken ct) => Ok(new { id = await categories.CreateAsync(input, ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryInput input, CancellationToken ct)
    {
        await categories.UpdateAsync(id, input, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await categories.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/image")]
    public async Task<IActionResult> Image(int id, IFormFile file, CancellationToken ct) =>
        Ok(new { url = await categories.SetImageAsync(id, file.ToUpload(), ct) });
}

[Route("api/admin/brands")]
public class AdminBrandsController(AdminBrandService brands) : AdminControllerBase
{
    [HttpGet]
    public Task<List<AdminBrandDto>> List(CancellationToken ct) => brands.ListAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Create(BrandInput input, CancellationToken ct) => Ok(new { id = await brands.CreateAsync(input, ct) });

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BrandInput input, CancellationToken ct)
    {
        await brands.UpdateAsync(id, input, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await brands.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/logo")]
    public async Task<IActionResult> Logo(int id, IFormFile file, CancellationToken ct) =>
        Ok(new { url = await brands.SetLogoAsync(id, file.ToUpload(), ct) });
}

[Route("api/admin/orders")]
public class AdminOrdersController(AdminOrderService orders) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResult<AdminOrderRowDto>> List([FromQuery] AdminOrderQuery query, CancellationToken ct) => orders.ListAsync(query, ct);

    [HttpGet("{id:int}")]
    public Task<OrderDetailDto> Get(int id, CancellationToken ct) => orders.GetAsync(id, ct);

    [HttpPut("{id:int}/status")]
    public Task<OrderDetailDto> UpdateStatus(int id, UpdateOrderStatusRequest request, CancellationToken ct) => orders.UpdateStatusAsync(id, request, ct);
}

[Route("api/admin/users")]
public class AdminUsersController(AdminUserService users) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResult<AdminUserDto>> List([FromQuery] AdminUserQuery query, CancellationToken ct) => users.ListAsync(query, ct);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request, CancellationToken ct)
    {
        await users.UpdateAsync(id, request, ct);
        return NoContent();
    }
}

[Route("api/admin/reviews")]
public class AdminReviewsController(AdminReviewService reviews) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResult<AdminReviewDto>> List([FromQuery] AdminReviewQuery query, CancellationToken ct) => reviews.ListAsync(query, ct);

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        await reviews.SetApprovedAsync(id, true, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        await reviews.SetApprovedAsync(id, false, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await reviews.DeleteAsync(id, ct);
        return NoContent();
    }
}

[Route("api/admin/banners")]
public class AdminBannersController(AdminBannerService banners) : AdminControllerBase
{
    [HttpGet]
    public Task<List<AdminBannerDto>> List(CancellationToken ct) => banners.ListAsync(ct);

    [HttpPost]
    public Task<AdminBannerDto> Create([FromForm] BannerInput input, IFormFile image, CancellationToken ct) =>
        banners.CreateAsync(input, image.ToUpload(), ct);

    [HttpPut("{id:int}")]
    public Task<AdminBannerDto> Update(int id, [FromForm] BannerInput input, IFormFile? image, CancellationToken ct) =>
        banners.UpdateAsync(id, input, image?.ToUpload(), ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await banners.DeleteAsync(id, ct);
        return NoContent();
    }
}

[Route("api/admin/coupons")]
public class AdminCouponsController(AdminCouponService coupons) : AdminControllerBase
{
    [HttpGet]
    public Task<List<CouponDto>> List(CancellationToken ct) => coupons.ListAsync(ct);

    [HttpPost]
    public Task<CouponDto> Create(CouponInput input, CancellationToken ct) => coupons.CreateAsync(input, ct);

    [HttpPut("{id:int}")]
    public Task<CouponDto> Update(int id, CouponInput input, CancellationToken ct) => coupons.UpdateAsync(id, input, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await coupons.DeleteAsync(id, ct);
        return NoContent();
    }
}
