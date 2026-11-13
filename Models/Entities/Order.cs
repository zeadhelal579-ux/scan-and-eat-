// FILE: Models/Entities/Order.cs
// PURPOSE: Mirrors the "Order" table from 03-DATABASE-SCHEMA.md section 2.
// LOGIC:   - Status is a strongly-typed OrderStatus enum in C# (safe,
//            IntelliSense-checked state transitions in OrderService), but
//            Data/AppDbContext.cs configures it with HasConversion<string>()
//            so it is still stored as TEXT in the actual database column -
//            "trades a few bytes of storage for a database that's actually
//            readable when you open it directly", exactly as
//            03-DATABASE-SCHEMA.md section 5 specifies.
//          - Subtotal/Tax/Total are always computed server-side by
//            OrderService from the real Product/Offer prices at the moment
//            of ordering - never trusted from the client.
//          - UpdatedAt is stamped on every status change (not just at
//            creation) - alone enough to answer "how long did this table
//            wait" later without a separate status-history table, per
//            07-REALTIME-ORDER-FLOW.md section 5.

using ScanEat.Api.Models.Enums;

namespace ScanEat.Api.Models.Entities;

public class Order
{
    public int Id { get; set; }

    public int TableId { get; set; }
    public RestaurantTable? Table { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation: every line item that makes up this order.
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
