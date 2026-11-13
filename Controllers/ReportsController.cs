// FILE: Controllers/ReportsController.cs
// PURPOSE: Admin - best sellers, sales summary
//          (04-BACKEND-ARCHITECTURE.md section 2's own description of this
//          file). Routes verified against admin/js/dashboard.js.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/admin/reports/best-sellers?days=30 - same computation as the
    // public best-sellers endpoint, with full numbers for the owner.
    [HttpGet("best-sellers")]
    public async Task<ActionResult<List<ProductDto>>> GetBestSellers([FromQuery] int days = 30)
    {
        // security-test-report.md A3 - see the matching comment in
        // Controllers/ProductsController.cs's GetBestSellers.
        days = Math.Clamp(days, 1, 365);
        return Ok(await _reportService.GetBestSellersAsync(days));
    }

    // GET /api/admin/reports/sales-summary?from=&to=
    [HttpGet("sales-summary")]
    public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var today = DateTime.UtcNow.Date;
        var fromDate = from ?? today;
        var toDate = to ?? today;
        return Ok(await _reportService.GetSalesSummaryAsync(fromDate, toDate));
    }
}
