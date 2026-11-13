// FILE: Models/Entities/Product.cs
// PURPOSE: Mirrors the "Product" table from 03-DATABASE-SCHEMA.md section 2.
// LOGIC:   - Price is the source of truth for every order - OrderService
//            always re-reads it from the database and never trusts a price
//            sent by the browser (02-SYSTEM-ARCHITECTURE.md section 5,
//            04-BACKEND-ARCHITECTURE.md section 4).
//          - IsAvailable is the one-tap "Sold Out" switch in the Admin
//            Dashboard (PATCH /api/admin/products/{id}/availability); the
//            customer app hides the "Add to Cart" button and shows a
//            "Sold Out" badge instead when this is false, rather than
//            removing the product from the list entirely.
//          - "Best Seller" is deliberately NOT a column here - it is
//            computed on demand from OrderItem history by ProductService /
//            ReportService, exactly as 03-DATABASE-SCHEMA.md specifies.

namespace ScanEat.Api.Models.Entities;

public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    // Navigation: every order line and offer breakdown line that references this product.
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OfferItem> OfferItems { get; set; } = new List<OfferItem>();
}
