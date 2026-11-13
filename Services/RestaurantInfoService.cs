// FILE: Services/RestaurantInfoService.cs
// PURPOSE: Business logic behind GET /api/restaurant-info
//          (08-API-CONTRACT.md section 1).
// LOGIC:   Certificates is stored as one newline-separated string on the
//          entity (03-DATABASE-SCHEMA.md: "Newline- or JSON-separated
//          list") and split into a string[] here, matching what
//          customer/js/app.js renderHome() reads
//          (`(info.certificates || []).forEach(...)`).

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Models.DTOs;

namespace ScanEat.Api.Services;

public class RestaurantInfoService : IRestaurantInfoService
{
    private readonly AppDbContext _db;

    public RestaurantInfoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RestaurantInfoDto> GetAsync()
    {
        var info = await _db.RestaurantInfos.FirstOrDefaultAsync();
        if (info is null)
        {
            // Defensive fallback only - Data/SeedData.cs always creates this
            // single row on first run, so this should not normally happen.
            return new RestaurantInfoDto();
        }

        return new RestaurantInfoDto
        {
            AboutText = info.AboutText,
            OpeningHours = info.OpeningHours,
            Certificates = info.Certificates
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            EstablishedYear = info.EstablishedYear,
        };
    }
}
