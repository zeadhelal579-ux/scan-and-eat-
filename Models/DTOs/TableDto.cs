// FILE: Models/DTOs/TableDto.cs
// PURPOSE: The wire shapes for the RestaurantTable resource
//          (08-API-CONTRACT.md section 4), verified against admin/js/tables.js.
// LOGIC:   QrToken IS included here even though the current admin/js/tables.js
//          UI never displays it directly (the QR image itself is fetched
//          separately, as a protected PNG blob - see TablesController).
//          06-QR-CODE-AND-TABLE-FLOW.md section 2 says the token is
//          "Staff only" - i.e. safe for an authenticated Admin response,
//          just never exposed on the public customer-facing endpoints
//          (which this DTO is not used by).

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

// GET /api/admin/tables and POST /api/admin/tables response shape.
public class TableDto
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string QrToken { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// Request body for POST /api/admin/tables - the server generates QrToken;
// the admin only supplies the human-readable number.
public class CreateTableDto
{
    [Required, MaxLength(20)]
    public string TableNumber { get; set; } = string.Empty;
}

// Request body for PATCH /api/admin/tables/{id} - activate/deactivate.
public class UpdateTableActiveDto
{
    public bool IsActive { get; set; }
}
