// FILE: Controllers/CategoriesController.cs
// PURPOSE: Public read; Admin CRUD (04-BACKEND-ARCHITECTURE.md section 2's
//          own description of this file). Routes verified against
//          shared/js/api-client.js's CategoriesApi / CategoriesAdminApi.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET /api/categories - public, in display order.
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }

    // POST /api/admin/categories
    [HttpPost("admin/categories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryUpsertDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), created);
    }

    // PUT /api/admin/categories/{id}
    [HttpPut("admin/categories/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryUpsertDto dto)
    {
        return Ok(await _categoryService.UpdateAsync(id, dto));
    }

    // DELETE /api/admin/categories/{id}
    [HttpDelete("admin/categories/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
