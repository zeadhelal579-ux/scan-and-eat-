// FILE: Models/Entities/Offer.cs
// PURPOSE: Mirrors the "Offer" table from 03-DATABASE-SCHEMA.md section 2 -
//          combo deals shown on the Offers page (e.g. "Twin Burger Pack -
//          EGP 300 instead of EGP 400").
// LOGIC:   - IsActive lets Admin retire an offer without deleting its order
//            history (past OrderItems still reference it for receipts).
//          - IsFeatured marks the ONE offer shown in the "Best Offer"
//            highlight card on the customer Menu page - OfferService
//            enforces the "only one at a time" rule whenever an offer is
//            saved with IsFeatured = true (unsetting it on every other row),
//            matching the "marks the ONE offer" wording in the schema doc.
// EXTENSION: Badge (e.g. "Save 25%", "Pizza Night") is not in the original
//          03-DATABASE-SCHEMA.md table, but every offer card in the actual,
//          already-built customer/kitchen/admin frontend
//          (customer/js/demo-data.js, admin/js/admin-demo-data.js) carries
//          one, with a documented "Offer" fallback in the UI if it's ever
//          missing. Added here - nullable, so it's a compatible, additive
//          extension - to match what the delivered frontend actually
//          renders instead of leaving every offer card showing "Offer".

namespace ScanEat.Api.Models.Entities;

public class Offer
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal OldPrice { get; set; }

    public decimal NewPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    // See EXTENSION note above - nullable so it never breaks a client that doesn't send it.
    public string? Badge { get; set; }

    // Navigation: what the combo is made of, and every order line that ordered this combo.
    public ICollection<OfferItem> OfferItems { get; set; } = new List<OfferItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
