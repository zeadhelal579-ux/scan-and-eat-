// FILE: Services/IReportService.cs
// PURPOSE: Contract for the Admin reporting endpoints, used by ReportsController.

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface IReportService
{
    Task<List<ProductDto>> GetBestSellersAsync(int days);
    Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime from, DateTime to);
}
