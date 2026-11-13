// FILE: Models/Entities/OrderItem.cs
// PURPOSE: Mirrors the "OrderItem" table from 03-DATABASE-SCHEMA.md section 2 -
//          one row per item inside an order. An item is either a standalone
//          Product OR a combo Offer - never both - matching how the customer
//          app already treats "Add to Cart" and "Claim Offer" identically
//          (customer/js/app.js, the cart's `type` field is "product" or "offer").
// LOGIC:   - ProductId/OfferId are both nullable and mutually exclusive per
//            row: OrderService sets exactly one of them depending on the
//            request item's "type".
//          - ItemNameSnapshot and UnitPrice are copied in AT THE MOMENT OF
//            ORDERING, not looked up live every time the order is displayed -
//            so a later renamed/deleted product or offer never corrupts a
//            historical order or receipt. This is also why deleting a
//            Product/Offer is safe (see the OnDelete(SetNull) configuration
//            in Data/AppDbContext.cs).
//          - Notes exists now (nullable) so Phase 2's per-item special
//            instructions ("no onion") don't require a schema migration -
//            the input UI for it is deferred, per 01-PROJECT-OVERVIEW.md
//            section 6. The field is already wired end-to-end here: the
//            CreateOrderDto accepts it and the Kitchen Display already
//            renders it per order line (kitchen/js/kitchen.js renderCard()).

namespace ScanEat.Api.Models.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // Set when this line is a standalone product. Nullable - see class header comment.
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Set when this line is a combo offer. Nullable - see class header comment.
    public int? OfferId { get; set; }
    public Offer? Offer { get; set; }

    public string ItemNameSnapshot { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? ComponentsJson { get; set; }

    public string? Notes { get; set; }
}
