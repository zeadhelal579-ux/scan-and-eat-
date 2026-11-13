// FILE: Models/Entities/RestaurantInfo.cs
// PURPOSE: Mirrors the "RestaurantInfo" table from 03-DATABASE-SCHEMA.md
//          section 2 - a single-row table backing the Home page content
//          (about text, hours, certificates) so Admin can edit it without a
//          code deployment. Only GET /api/restaurant-info is in Phase 1's
//          API contract (08-API-CONTRACT.md section 1) - no admin write
//          endpoint yet, matching the schema doc's note that it's "fine to
//          launch with these values hardcoded ... migrate whenever
//          convenient". The table itself is still built now so that future
//          admin-editing endpoint is a pure addition, not a migration.
// LOGIC:   - Certificates is stored as a single newline-separated string
//            (matches "Newline- or JSON-separated list" from the schema
//            doc); Services/RestaurantInfoService.cs splits it into the
//            string[] the customer app's renderHome() expects.
// EXTENSION: EstablishedYear is not in the original schema table, but the
//          already-built customer app's renderHome() reads
//          `info.establishedYear` directly (customer/js/app.js) to show
//          "Established in {year}" under the About text. Added here -
//          nullable int - as a small, additive extension so that field
//          isn't silently blank forever.

namespace ScanEat.Api.Models.Entities;

public class RestaurantInfo
{
    // Always a single row (see class header comment).
    public int Id { get; set; }

    public string AboutText { get; set; } = string.Empty;

    public string OpeningHours { get; set; } = string.Empty;

    public string Certificates { get; set; } = string.Empty;

    public int? EstablishedYear { get; set; }
}
