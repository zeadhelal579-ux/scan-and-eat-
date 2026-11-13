// FILE: Controllers/OrdersController.cs
// PURPOSE: Customer submit; Staff list/update status
//          (04-BACKEND-ARCHITECTURE.md section 2's own description of this
//          file). Routes verified against customer/js/api.js (OrdersApi)
//          and shared/js/api-client.js (StaffOrdersApi).
// LOGIC:   [Authorize(Roles = "Staff,Admin")] uses ASP.NET Core's built-in
//          comma-separated OR semantics for the Roles property - a Staff OR
//          an Admin token is accepted, matching
//          shared/js/auth.js guardPage({ requireRole: "Staff" })'s own rule
//          that Admin is treated as a superset of Staff.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Services;

namespace ScanEat.Api.Controllers;

[ApiController]
[Route("api")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST /api/orders - public, the customer's "Send Order to Kitchen" button.
    [HttpPost("orders")]
    [AllowAnonymous]
    [EnableRateLimiting("orders")] // security-test-report.md F7
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CreateOrderAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // GET /api/staff/orders?status=Pending,InPreparation - used on Kitchen
    // Display page load, before SignalR takes over.
    [HttpGet("staff/orders")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<List<OrderDetailDto>>> GetOpenOrders([FromQuery] string? status)
    {
        return Ok(await _orderService.GetOrdersAsync(status));
    }

    // PATCH /api/staff/orders/{id}/status - advances the order state machine
    // and triggers the OrderStatusChanged broadcast.
    [HttpPatch("staff/orders/{id:int}/status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        return Ok(await _orderService.UpdateStatusAsync(id, dto.Status));
    }
}
