// FILE: Services/IOrderService.cs
// PURPOSE: Contract for Order business logic, used by OrdersController.
//          This is "the most important single piece of the backend"
//          (09-ROADMAP.md step 6).

using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public interface IOrderService
{
    // Customer-facing: validates the table token, recalculates every price
    // from the database, saves the order, and broadcasts NewOrder.
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);

    // Staff-facing: loads currently open orders (or a specific status set).
    Task<List<OrderDetailDto>> GetOrdersAsync(string? statusCsv);

    // Staff-facing: advances an order's status and broadcasts OrderStatusChanged.
    Task<OrderDetailDto> UpdateStatusAsync(int orderId, string newStatusRaw);
}
