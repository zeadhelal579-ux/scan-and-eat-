// FILE: Models/DTOs/OrderDtos.cs
// PURPOSE: Every wire shape around the Order resource
//          (08-API-CONTRACT.md sections 1, 5 and 7), verified field-for-field
//          against the actual frontend code that builds/reads them:
//            - CreateOrderDto        <- customer/js/app.js submitOrder()
//            - OrderResponseDto      -> read by customer/js/app.js submitOrder()
//            - OrderDetailDto/Item   -> read by kitchen/js/kitchen.js renderCard()
//                                       and reused as the NewOrder SignalR payload
//            - UpdateOrderStatusDto  <- kitchen/js/kitchen.js handleAdvance()
// LOGIC:   Status fields are plain strings on every DTO here (never the
//          OrderStatus enum type directly) so JSON always serializes them as
//          "Pending"/"InPreparation"/etc. with zero risk of System.Text.Json's
//          default numeric enum serialization slipping through - the
//          service layer is the only place that works with the real
//          OrderStatus enum, converting at the boundary.

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

// ---- POST /api/orders (customer submits an order) ------------------------

// Request body. tableToken is the RestaurantTable.QrToken read from the
// scanned QR link's "?table=" parameter - never a plain table number.
public class CreateOrderDto
{
    // security-test-report.md F8: QrToken is a Guid (TableService.cs), so
    // 64 chars is already generous headroom, not a tight fit.
    [Required, MaxLength(64)]
    public string TableToken { get; set; } = string.Empty;

    // MaxLength(50): a real table's single order realistically has a
    // handful of line items; 50 comfortably covers a large group order
    // while still ruling out a request trying to force the server to
    // price out thousands of lines in one call.
    [Required, MinLength(1, ErrorMessage = "An order needs at least one item."), MaxLength(50)]
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

// One requested line. Type is "product" or "offer" (case-insensitive),
// matching the literal strings the customer app's data-type attributes
// already send - see customer/js/app.js's cart entries.
public class OrderItemRequestDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    public int Id { get; set; }

    // security-test-report.md F8: was Range(1, int.MaxValue) - unbounded
    // above 1 let a single line request an absurd quantity. 50 is well
    // above anything a real table would order in one line.
    [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50.")]
    public int Quantity { get; set; } = 1;

    [MaxLength(300)]
    public string? Notes { get; set; }
}

// 201 response returned straight after a successful submit.
public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

// ---- GET /api/staff/orders + the NewOrder SignalR payload ----------------

// Full order detail as the Kitchen Display needs it. This EXACT shape is
// used both for the initial GET /api/staff/orders list (so a page refresh
// never loses an in-progress order) and for the OrderHub "NewOrder"
// broadcast - kitchen.js stores whichever one arrives first into the same
// `orders` Map and renders both through the identical renderCard() path,
// so the two must never drift apart.
public class OrderDetailDto
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<OrderDetailItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderDetailItemDto
{
    public int Quantity { get; set; }
    public string ItemNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public List<OrderComponentDto> Components { get; set; } = [];
    public string? Notes { get; set; }
}

public record OrderComponentDto(string Name, long Quantity);

// ---- PATCH /api/staff/orders/{id}/status ----------------------------------

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
