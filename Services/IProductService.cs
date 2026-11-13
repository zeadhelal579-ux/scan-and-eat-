// FILE: Services/IProductService.cs
// PURPOSE: Contract for Product business logic, used by ProductsController
//          (public read + best-sellers) and ReportService (admin best-sellers
//          reuses the exact same computation - see ProductService.cs).

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int? categoryId, string? search);
    Task<ProductDto?> GetByIdAsync(int id);

    // count = null means "no limit" (used by the admin report, which shows
    // every ordered product in the window rather than a small highlight list).
    Task<List<ProductDto>> GetBestSellersAsync(int days, int? count);

    Task<ProductDto> CreateAsync(ProductUpsertDto dto);
    Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto);
    Task<ProductDto> SetAvailabilityAsync(int id, bool isAvailable);
    Task DeleteAsync(int id);
}
