// FILE: Services/CategoryService.cs
// PURPOSE: Business logic for the Category resource
//          (04-BACKEND-ARCHITECTURE.md section 3 - "all business logic
//          lives here, Controllers never talk to AppDbContext directly").
// LOGIC:   - GetAllAsync always orders by DisplayOrder: the customer app's
//            renderCategories() (customer/js/app.js) renders categories in
//            exactly the order the API returns them, with no client-side
//            sort of its own - so the API is the only place this ordering
//            can correctly happen.
//          - DeleteAsync blocks deleting a category that still has products
//            in it with a clear message instead of letting a raw database
//            foreign-key error surface - matching the warning already shown
//            in the Admin Dashboard's own confirm dialog ("Products inside
//            it should be moved first" - admin/js/categories.js).

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CategoryDto> CreateAsync(CategoryUpsertDto dto)
    {
        var category = new Category
        {
            Name = dto.Name.Trim(),
            ImageUrl = dto.ImageUrl.Trim(),
            DisplayOrder = dto.DisplayOrder,
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw ApiException.NotFound($"Category #{id} was not found.");

        category.Name = dto.Name.Trim();
        category.ImageUrl = dto.ImageUrl.Trim();
        category.DisplayOrder = dto.DisplayOrder;
        await _db.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id)
            ?? throw ApiException.NotFound($"Category #{id} was not found.");

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            throw ApiException.Conflict(
                "This category still has products in it. Move or delete those products first.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    // Maps the Category entity to its wire shape.
    private static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        ImageUrl = c.ImageUrl,
        DisplayOrder = c.DisplayOrder,
    };
}
