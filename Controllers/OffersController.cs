// FILE: Controllers/OffersController.cs
// PURPOSE: Public read; Admin CRUD (04-BACKEND-ARCHITECTURE.md section 2's
//          own description of this file). Routes verified against
//          shared/js/api-client.js's OffersApi / OffersAdminApi and
//          customer/js/api.js.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;

    public OffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    // GET /api/offers - public, includes which one is IsFeatured.
    [HttpGet("offers")]
    [AllowAnonymous]
    public async Task<ActionResult<List<OfferDto>>> GetAll()
    {
        return Ok(await _offerService.GetAllAsync());
    }

    // POST /api/admin/offers
    [HttpPost("admin/offers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OfferDto>> Create([FromBody] OfferUpsertDto dto)
    {
        var created = await _offerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), created);
    }

    // PUT /api/admin/offers/{id}
    [HttpPut("admin/offers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OfferDto>> Update(int id, [FromBody] OfferUpsertDto dto)
    {
        return Ok(await _offerService.UpdateAsync(id, dto));
    }

    // DELETE /api/admin/offers/{id}
    [HttpDelete("admin/offers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _offerService.DeleteAsync(id);
        return NoContent();
    }
}
