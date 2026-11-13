// FILE: Models/DTOs/ReportDtos.cs
// PURPOSE: The wire shape for GET /api/admin/reports/sales-summary
//          (08-API-CONTRACT.md section 6, marked "Phase 2" there but
//          implemented here - see ReportService.cs header comment for why),
//          verified against admin/js/dashboard.js loadSummaryTiles().
//          The best-sellers report reuses ProductDto (with OrderCount
//          populated) rather than a separate type - see ProductDto.cs.

namespace ScanEat.Api.Models.DTOs;

public class SalesSummaryDto
{
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal AverageOrderValue { get; set; }
}
