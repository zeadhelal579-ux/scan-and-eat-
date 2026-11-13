// FILE: Models/Entities/RestaurantTable.cs
// PURPOSE: Mirrors the "RestaurantTable" table from 03-DATABASE-SCHEMA.md
//          section 2 - one row per physical table.
// LOGIC:   - TableNumber is the human-readable label staff see (printed near
//            the QR code so a person can tell tables apart).
//          - QrToken is the long, random, unguessable value encoded INSIDE
//            the QR link instead - not TableNumber - so a customer editing
//            "?table=1" to "?table=2" in the address bar cannot land an
//            order on someone else's table. See
//            06-QR-CODE-AND-TABLE-FLOW.md section 2 for the full reasoning.
//          - IsActive lets Admin deactivate a removed/merged table without
//            breaking historical Orders that still reference it (there is
//            deliberately no DELETE endpoint for tables in
//            08-API-CONTRACT.md section 4 - deactivation is the only
//            supported removal path).

namespace ScanEat.Api.Models.Entities;

public class RestaurantTable
{
    public int Id { get; set; }

    public string TableNumber { get; set; } = string.Empty;

    public string QrToken { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation: every order ever placed at this table.
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
