// FILE: Services/ProductService.cs
// PURPOSE: Business logic for the Product resource
//          (04-BACKEND-ARCHITECTURE.md section 3).
// LOGIC:   - GetBestSellersAsync is the one method behind BOTH
//            GET /api/products/best-sellers (public, small/highlight) and
//            GET /api/admin/reports/best-sellers (admin, full numbers) -
//            ReportService.cs calls this with count:null for the admin
//            version, matching 08-API-CONTRACT.md's own note that the admin
//            report is "Same computation as the public best-sellers
//            endpoint, with full numbers for the owner". "Best Seller" is
//            deliberately NOT a stored flag anywhere - it's computed here,
//            fresh, from real OrderItem history every time, exactly as
//            03-DATABASE-SCHEMA.md specifies.
//          - DeleteAsync blocks deleting a product that's still part of a
//            combo offer's breakdown, with a clear message, rather than
//            letting a raw foreign-key error surface.

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;
using ScanEat.Api.Models.Enums;

namespace ScanEat.Api.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductDto>> GetAllAsync(int? categoryId, string? search)
    {
        IQueryable<Product> query = _db.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // Matches name OR description, same as the customer app's own
            // client-side search fallback (customer/js/demo-data.js
            // demoProductList()) - relies on SQL Server's default
            // case-insensitive collation for a case-insensitive match.
            query = query.Where(p => p.Name.Contains(term) || p.Description.Contains(term));
        }

        return await query.OrderBy(p => p.Id).Select(p => ToDto(p)).ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        return product is null ? null : ToDto(product);
    }

    public async Task<List<ProductDto>> GetBestSellersAsync(int days, int? count)
    {
        var cutoff = DateTime.UtcNow.AddDays(-Math.Abs(days));

        // Sum of quantities ordered per product, in the window, excluding
        // cancelled orders (an order that never got fulfilled shouldn't
        // count toward "best selling").
        var grouped = await _db.OrderItems
            .Where(oi => oi.ProductId != null
                         && oi.Order!.CreatedAt >= cutoff
                         && oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, OrderCount = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.OrderCount)
            .ToListAsync();

        if (count.HasValue)
        {
            grouped = grouped.Take(count.Value).ToList();
        }

        if (grouped.Count == 0)
        {
            return new List<ProductDto>();
        }

        var productIds = grouped.Select(g => g.ProductId!.Value).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var result = new List<ProductDto>();
        foreach (var g in grouped)
        {
            if (!products.TryGetValue(g.ProductId!.Value, out var product))
            {
                continue; // product was deleted since it was ordered - skip it
            }
            var dto = ToDto(product);
            dto.OrderCount = g.OrderCount;
            result.Add(dto);
        }
        return result;
    }

    public async Task<ProductDto> CreateAsync(ProductUpsertDto dto)
    {
        await EnsureCategoryExistsAsync(dto.CategoryId);

        var product = new Product
        {
            Name = dto.Name.Trim(),
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Description = dto.Description.Trim(),
            ImageUrl = dto.ImageUrl.Trim(),
            IsAvailable = true,
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return ToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw ApiException.NotFound($"Product #{id} was not found.");

        await EnsureCategoryExistsAsync(dto.CategoryId);

        product.Name = dto.Name.Trim();
        product.CategoryId = dto.CategoryId;
        product.Price = dto.Price;
        product.Description = dto.Description.Trim();
        product.ImageUrl = dto.ImageUrl.Trim();
        await _db.SaveChangesAsync();
        return ToDto(product);
    }

    public async Task<ProductDto> SetAvailabilityAsync(int id, bool isAvailable)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw ApiException.NotFound($"Product #{id} was not found.");

        product.IsAvailable = isAvailable;
        await _db.SaveChangesAsync();
        return ToDto(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw ApiException.NotFound($"Product #{id} was not found.");

        var usedInOffer = await _db.OfferItems.AnyAsync(oi => oi.ProductId == id);
        if (usedInOffer)
        {
            throw ApiException.Conflict(
                "This product is part of one or more combo offers. Remove it from those offers first.");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Id == categoryId);
        if (!exists)
        {
            throw ApiException.BadRequest($"Category #{categoryId} does not exist.");
        }
    }

    // Maps the Product entity to its wire shape. OrderCount is left null
    // here - only GetBestSellersAsync populates it.
    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        CategoryId = p.CategoryId,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        ImageUrl = p.ImageUrl,
        IsAvailable = p.IsAvailable,
    };
}
