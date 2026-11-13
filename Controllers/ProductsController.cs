// FILE: Controllers/ProductsController.cs
// PURPOSE: Public read + best-sellers; Admin CRUD + sold-out toggle
//          (04-BACKEND-ARCHITECTURE.md section 2's own description of this
//          file). Routes verified against shared/js/api-client.js's
//          ProductsApi / ProductsAdminApi and customer/js/api.js.
// LOGIC:   The "{id:int}" constraint on the single-product route is what
//          lets "products/best-sellers" register as its own literal route
//          without ever being swallowed by "products/{id}" - see
//          ProductDto.cs / IProductService.cs header comments for the
//          best-sellers computation itself.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET /api/products?category=&search=
    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        int? categoryId = int.TryParse(category, out var parsed) ? parsed : null;
        return Ok(await _productService.GetAllAsync(categoryId, search));
    }

    // GET /api/products/best-sellers?days=30&count=5 - must be registered
    // ahead of nothing in particular (the :int constraint below already
    // disambiguates it), kept first here purely for readability.
    [HttpGet("products/best-sellers")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductDto>>> GetBestSellers([FromQuery] int days = 30, [FromQuery] int count = 5)
    {
        // security-test-report.md A3: this is a public, unauthenticated
        // endpoint. Unclamped, a very large/negative `days` reached a
        // DateTime.AddDays() call outside .NET's year 1-9999 range and threw
        // an unhandled exception (500) instead of a normal response -
        // clamping here means the worst a caller can do is ask for a wider
        // or narrower window than makes sense, which just returns fewer
        // rows, never an error.
        days = Math.Clamp(days, 1, 365);
        count = Math.Clamp(count, 1, 50);
        return Ok(await _productService.GetBestSellersAsync(days, count));
    }

    // GET /api/products/{id}
    [HttpGet("products/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null
            ? NotFound(new { message = $"Product #{id} was not found." })
            : Ok(product);
    }

    // POST /api/admin/products
    [HttpPost("admin/products")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductUpsertDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/admin/products/{id}
    [HttpPut("admin/products/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] ProductUpsertDto dto)
    {
        return Ok(await _productService.UpdateAsync(id, dto));
    }

    // PATCH /api/admin/products/{id}/availability - the one-tap Sold Out toggle.
    [HttpPatch("admin/products/{id:int}/availability")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> SetAvailability(int id, [FromBody] UpdateProductAvailabilityDto dto)
    {
        return Ok(await _productService.SetAvailabilityAsync(id, dto.IsAvailable));
    }

    // DELETE /api/admin/products/{id}
    [HttpDelete("admin/products/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
