// FILE: Services/TableService.cs
// PURPOSE: Business logic for the RestaurantTable resource
//          (04-BACKEND-ARCHITECTURE.md section 3).
// LOGIC:   - CreateAsync is the ONLY place a QrToken is ever generated - the
//            admin only supplies the human-readable TableNumber
//            (06-QR-CODE-AND-TABLE-FLOW.md: "the admin never types one in,
//            it's unguessable, not chosen by a human"). Guid.NewGuid() is
//            cryptographically unpredictable, matching that requirement.
//          - There is deliberately no DeleteAsync here: 08-API-CONTRACT.md
//            section 4 has no DELETE route for tables, only
//            PATCH .../{id} to activate/deactivate - see
//            03-DATABASE-SCHEMA.md's RestaurantTable notes on why
//            (deactivating keeps historical Orders intact).

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Services;

public class TableService : ITableService
{
    private readonly AppDbContext _db;

    public TableService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetActiveTableNumberAsync(string token)
    {
        return await _db.RestaurantTables.Where(table => table.QrToken == token && table.IsActive)
            .Select(table => table.TableNumber).FirstOrDefaultAsync()
            ?? throw ApiException.NotFound("This table link is unavailable. Ask staff for an active table QR.");
    }

    public async Task<List<TableDto>> GetAllAsync()
    {
        return await _db.RestaurantTables
            .OrderBy(t => t.Id)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TableDto> CreateAsync(CreateTableDto dto)
    {
        var tableNumber = dto.TableNumber.Trim();
        if (await _db.RestaurantTables.AnyAsync(table => table.TableNumber == tableNumber))
        {
            throw ApiException.Conflict("That table already exists.");
        }
        var table = new RestaurantTable
        {
            TableNumber = tableNumber,
            QrToken = Guid.NewGuid().ToString(),
            IsActive = true,
        };
        _db.RestaurantTables.Add(table);
        await _db.SaveChangesAsync();
        return ToDto(table);
    }

    public async Task<TableDto> SetActiveAsync(int id, bool isActive)
    {
        var table = await _db.RestaurantTables.FindAsync(id)
            ?? throw ApiException.NotFound($"Table #{id} was not found.");

        table.IsActive = isActive;
        await _db.SaveChangesAsync();
        return ToDto(table);
    }

    public async Task<RestaurantTable> GetEntityOrThrowAsync(int id)
    {
        return await _db.RestaurantTables.FindAsync(id)
            ?? throw ApiException.NotFound($"Table #{id} was not found.");
    }

    private static TableDto ToDto(RestaurantTable t) => new()
    {
        Id = t.Id,
        TableNumber = t.TableNumber,
        QrToken = t.QrToken,
        IsActive = t.IsActive,
    };
}
