// FILE: Services/ReportService.cs
// PURPOSE: Business logic behind Controllers/ReportsController.cs
//          (04-BACKEND-ARCHITECTURE.md section 2).
// LOGIC:   - GetBestSellersAsync delegates to IProductService's own
//            best-sellers computation with count:null ("no limit") - see
//            08-API-CONTRACT.md section 6: "Same computation as the public
//            best-sellers endpoint, with full numbers for the owner". There
//            is deliberately only ONE implementation of "what counts as a
//            best seller" in the whole codebase (ProductService.cs).
//          - GetSalesSummaryAsync is listed as "Phase 2" in
//            08-API-CONTRACT.md section 6, and the already-built Admin
//            Dashboard (admin/js/dashboard.js) already calls it today and
//            gracefully degrades its three tiles to "—" on a 404. It is
//            implemented here anyway - it is a small, natural aggregate
//            query over data Phase 1 already has (Order.Total, CreatedAt,
//            Status), and implementing it now means the dashboard shows
//            real numbers immediately instead of three permanent dashes.
//            Cancelled orders are excluded - they were never actually
//            fulfilled, so they shouldn't count as revenue or as a completed order.

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Enums;

namespace ScanEat.Api.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IProductService _productService;

    public ReportService(AppDbContext db, IProductService productService)
    {
        _db = db;
        _productService = productService;
    }

    public Task<List<ProductDto>> GetBestSellersAsync(int days)
    {
        return _productService.GetBestSellersAsync(days, count: null);
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime from, DateTime to)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1); // exclusive upper bound - covers the whole "to" day

        var orders = await _db.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end && o.Status != OrderStatus.Cancelled)
            .Select(o => o.Total)
            .ToListAsync();

        var ordersCount = orders.Count;
        var revenue = orders.Sum();
        var average = ordersCount > 0 ? Math.Round(revenue / ordersCount, 2, MidpointRounding.AwayFromZero) : 0m;

        return new SalesSummaryDto
        {
            OrdersToday = ordersCount,
            RevenueToday = revenue,
            AverageOrderValue = average,
        };
    }
}
