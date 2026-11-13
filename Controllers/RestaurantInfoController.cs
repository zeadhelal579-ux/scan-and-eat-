// FILE: Controllers/RestaurantInfoController.cs
// PURPOSE: Backs GET /api/restaurant-info (08-API-CONTRACT.md section 1),
//          which the Home page needs (customer/js/app.js renderHome()).
// EXTENSION: 04-BACKEND-ARCHITECTURE.md section 2's Controllers/ listing
//          doesn't name this file explicitly, but 08-API-CONTRACT.md
//          clearly requires the route, and the already-built customer app
//          already calls it - so this is a small, necessary addition
//          matching the documented API contract, following the same
//          one-controller-per-resource pattern every other controller here uses.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api/restaurant-info")]
public class RestaurantInfoController : ControllerBase
{
    private readonly IRestaurantInfoService _restaurantInfoService;

    public RestaurantInfoController(IRestaurantInfoService restaurantInfoService)
    {
        _restaurantInfoService = restaurantInfoService;
    }

    // GET /api/restaurant-info - public, backs the customer app's Home page.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantInfoDto>> Get()
    {
        return Ok(await _restaurantInfoService.GetAsync());
    }
}
