// FILE: Models/Entities/OfferItem.cs
// PURPOSE: Mirrors the "OfferItem" table from 03-DATABASE-SCHEMA.md section 2 -
//          what an offer is actually MADE OF. Without this, the kitchen would
//          only see the label "Twin Burger Pack" with no way to know it means
//          two cheeseburgers and one fries. Same "junction table" idea as
//          OrderItem, one level up.

namespace ScanEat.Api.Models.Entities;

public class OfferItem
{
    public int Id { get; set; }

    public int OfferId { get; set; }
    public Offer? Offer { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
}
