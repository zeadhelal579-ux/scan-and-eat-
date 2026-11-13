// FILE: Controllers/TablesController.cs
// PURPOSE: Admin only - CRUD + QR code image
//          (04-BACKEND-ARCHITECTURE.md section 2's own description of this
//          file). Routes verified against admin/js/tables.js.
// LOGIC:   The whole controller is [Authorize(Roles = "Admin")] at the
//          class level - every route here is under api/admin/tables, so
//          there is no public/admin split to make per-action the way
//          Categories/Products/Offers need.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api/admin/tables")]
[Authorize(Roles = "Admin")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;
    private readonly IQrCodeService _qrCodeService;

    public TablesController(ITableService tableService, IQrCodeService qrCodeService)
    {
        _tableService = tableService;
        _qrCodeService = qrCodeService;
    }

    [HttpGet("/api/tables/current")]
    [AllowAnonymous]
    public async Task<IActionResult> ResolveTable([FromQuery] Guid token)
    {
        var tableNumber = await _tableService.GetActiveTableNumberAsync(token.ToString());
        return Ok(new { tableNumber });
    }

    // GET /api/admin/tables
    [HttpGet]
    public async Task<ActionResult<List<TableDto>>> GetAll()
    {
        return Ok(await _tableService.GetAllAsync());
    }

    // POST /api/admin/tables - server generates the QrToken.
    [HttpPost]
    public async Task<ActionResult<TableDto>> Create([FromBody] CreateTableDto dto)
    {
        var created = await _tableService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), created);
    }

    // GET /api/admin/tables/{id}/qr-code - the printable QR code PNG.
    [HttpGet("{id:int}/qr-code")]
    public async Task<IActionResult> GetQrCode(int id)
    {
        var table = await _tableService.GetEntityOrThrowAsync(id);
        var png = _qrCodeService.GenerateTableQrPng(table.QrToken);
        return File(png, "image/png");
    }

    // PATCH /api/admin/tables/{id} - activate/deactivate.
    [HttpPatch("{id:int}")]
    public async Task<ActionResult<TableDto>> SetActive(int id, [FromBody] UpdateTableActiveDto dto)
    {
        return Ok(await _tableService.SetActiveAsync(id, dto.IsActive));
    }
}
