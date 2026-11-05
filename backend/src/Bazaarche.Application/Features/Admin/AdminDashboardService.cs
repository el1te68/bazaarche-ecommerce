using Bazaarche.Application.Common;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Admin;

public record DailySalesDto(string Date, long Revenue, int Orders);
public record StatusCountDto(OrderStatus Status, int Count);
public record RecentOrderDto(int Id, string OrderNumber, string CustomerName, long Amount, OrderStatus Status, DateTime CreatedAt);
public record LowStockDto(int Id, string Title, string Slug, int Stock, string? ImageUrl);
public record TopProductDto(int Id, string Title, string Slug, int SoldCount, string? ImageUrl);

public record DashboardDto(
    long TotalRevenue,
    int TotalOrders,
    int TotalCustomers,
    int TotalProducts,
    long TodayRevenue,
    int TodayOrders,
    int PendingReviews,
    List<DailySalesDto> Sales,
    List<StatusCountDto> OrderStatuses,
    List<RecentOrderDto> RecentOrders,
    List<LowStockDto> LowStock,
    List<TopProductDto> TopProducts);

public class AdminDashboardService(IAppDbContext db)
{
    // Iran Standard Time (no DST since 2022)
    private static readonly TimeSpan TehranOffset = TimeSpan.FromHours(3.5);

    public async Task<DashboardDto> GetAsync(CancellationToken ct)
    {
        var paidStatuses = new[] { OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered };
        var paid = db.Orders.AsNoTracking().Where(o => paidStatuses.Contains(o.Status));

        var totalRevenue = await paid.SumAsync(o => (long?)o.PayableAmount, ct) ?? 0;
        var totalOrders = await paid.CountAsync(ct);
        var customers = await db.Users.CountAsync(u => u.Role == UserRole.Customer, ct);
        var products = await db.Products.CountAsync(ct);
        var pendingReviews = await db.Reviews.CountAsync(r => !r.IsApproved, ct);

        var localToday = (DateTime.UtcNow + TehranOffset).Date;
        var from = DateTime.SpecifyKind(localToday.AddDays(-29) - TehranOffset, DateTimeKind.Utc);
        var recentPaid = await paid.Where(o => o.PaidAt >= from)
            .Select(o => new { o.PaidAt, o.PayableAmount }).ToListAsync(ct);

        var sales = Enumerable.Range(0, 30).Select(i =>
        {
            var day = localToday.AddDays(-29 + i);
            var ofDay = recentPaid.Where(o => (o.PaidAt!.Value + TehranOffset).Date == day).ToList();
            return new DailySalesDto(day.ToString("yyyy-MM-dd"), ofDay.Sum(o => o.PayableAmount), ofDay.Count);
        }).ToList();

        var statuses = await db.Orders.AsNoTracking().GroupBy(o => o.Status)
            .Select(g => new StatusCountDto(g.Key, g.Count())).ToListAsync(ct);

        var recent = await db.Orders.AsNoTracking().OrderByDescending(o => o.Id).Take(6)
            .Select(o => new RecentOrderDto(o.Id, o.OrderNumber, o.User!.FullName, o.PayableAmount, o.Status, o.CreatedAt))
            .ToListAsync(ct);

        var lowStock = await db.Products.AsNoTracking().Where(p => p.TotalStock <= 5).OrderBy(p => p.TotalStock).Take(6)
            .Select(p => new LowStockDto(p.Id, p.Title, p.Slug, p.TotalStock, p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()))
            .ToListAsync(ct);

        var top = await db.Products.AsNoTracking().Where(p => p.SoldCount > 0).OrderByDescending(p => p.SoldCount).Take(6)
            .Select(p => new TopProductDto(p.Id, p.Title, p.Slug, p.SoldCount, p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()))
            .ToListAsync(ct);

        return new DashboardDto(totalRevenue, totalOrders, customers, products,
            sales[^1].Revenue, sales[^1].Orders, pendingReviews, sales, statuses, recent, lowStock, top);
    }
}
