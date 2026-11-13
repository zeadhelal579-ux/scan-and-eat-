// FILE: Services/ICategoryService.cs
// PURPOSE: Contract for Category business logic, used by CategoriesController.
//          See CategoryService.cs for the implementation and reasoning.

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CategoryUpsertDto dto);
    Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto);
    Task DeleteAsync(int id);
}
