// FILE: Models/DTOs/RestaurantInfoDto.cs
// PURPOSE: The wire shape for GET /api/restaurant-info
//          (08-API-CONTRACT.md section 1), verified against
//          customer/js/app.js renderHome().
// LOGIC:   Certificates is a string[] here (never the raw newline-joined
//          text stored in the RestaurantInfo entity) - Services/
//          RestaurantInfoService.cs does the split when mapping to this DTO.

namespace ScanEat.Api.Models.DTOs;

public class RestaurantInfoDto
{
    public string AboutText { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public List<string> Certificates { get; set; } = new();
    public int? EstablishedYear { get; set; }
}
