// FILE: Services/ITableService.cs
// PURPOSE: Contract for RestaurantTable business logic, used by TablesController.

using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Services;

public interface ITableService
{
    Task<string> GetActiveTableNumberAsync(string token);
    Task<List<TableDto>> GetAllAsync();
    Task<TableDto> CreateAsync(CreateTableDto dto);
    Task<TableDto> SetActiveAsync(int id, bool isActive);

    // Returns the full entity (its QrToken is what the QR image is built
    // from) - throws ApiException.NotFound if the id doesn't exist.
    Task<RestaurantTable> GetEntityOrThrowAsync(int id);
}
