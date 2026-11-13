// FILE: Services/OfferService.cs
// PURPOSE: Business logic for the Offer resource
//          (04-BACKEND-ARCHITECTURE.md section 3).
// LOGIC:   - IsFeatured is enforced server-side as mutually exclusive across
//            every offer: 03-DATABASE-SCHEMA.md's own wording is "Marks the
//            ONE offer shown" - so saving an offer with IsFeatured = true
//            here always unsets it on every other offer first. (The already
//            -built Admin Dashboard already does the same thing optimistically
//            on its own side - admin/js/offers.js - this is the authoritative,
//            server-side version of that same rule.)
//          - Items (the OfferItem breakdown) is optional on write: if the
//            caller omits it, an existing breakdown is left untouched on
//            update, and a new offer is simply created with no breakdown yet
//            - matching the currently-shipped Admin Offers page, which has
//              no UI for editing it yet (see OfferDto.cs header comment).

using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Services;

public class OfferService : IOfferService
{
    private readonly AppDbContext _db;

    public OfferService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OfferDto>> GetAllAsync()
    {
        var offers = await _db.Offers
            .Include(o => o.OfferItems)
            .OrderBy(o => o.Id)
            .ToListAsync();
        return offers.Select(ToDto).ToList();
    }

    public async Task<OfferDto> CreateAsync(OfferUpsertDto dto)
    {
        if (dto.IsFeatured)
        {
            await UnfeatureAllAsync();
        }

        var offer = new Offer
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            OldPrice = dto.OldPrice,
            NewPrice = dto.NewPrice,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            Badge = string.IsNullOrWhiteSpace(dto.Badge) ? null : dto.Badge.Trim(),
        };

        if (dto.Items is { Count: > 0 })
        {
            await ValidateOfferItemsAsync(dto.Items);
            offer.OfferItems = dto.Items
                .Select(i => new OfferItem { ProductId = i.ProductId, Quantity = i.Quantity })
                .ToList();
        }

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync();
        return ToDto(offer);
    }

    public async Task<OfferDto> UpdateAsync(int id, OfferUpsertDto dto)
    {
        var offer = await _db.Offers.Include(o => o.OfferItems).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw ApiException.NotFound($"Offer #{id} was not found.");

        if (dto.IsFeatured)
        {
            await UnfeatureAllAsync(exceptOfferId: id);
        }

        offer.Title = dto.Title.Trim();
        offer.Description = dto.Description.Trim();
        offer.OldPrice = dto.OldPrice;
        offer.NewPrice = dto.NewPrice;
        offer.IsActive = dto.IsActive;
        offer.IsFeatured = dto.IsFeatured;
        offer.Badge = string.IsNullOrWhiteSpace(dto.Badge) ? null : dto.Badge.Trim();

        // Only replace the breakdown if the caller actually sent one -
        // see class header comment.
        if (dto.Items is not null)
        {
            await ValidateOfferItemsAsync(dto.Items);
            _db.OfferItems.RemoveRange(offer.OfferItems);
            offer.OfferItems = dto.Items
                .Select(i => new OfferItem { ProductId = i.ProductId, Quantity = i.Quantity })
                .ToList();
        }

        await _db.SaveChangesAsync();
        return ToDto(offer);
    }

    public async Task DeleteAsync(int id)
    {
        var offer = await _db.Offers.FindAsync(id)
            ?? throw ApiException.NotFound($"Offer #{id} was not found.");

        // Offer -> OfferItem is configured Cascade, and Order -> OrderItem's
        // OfferId is SetNull (see Data/AppDbContext.cs), so this is safe:
        // the combo's breakdown goes with it, and past receipts keep their
        // already-snapshotted name/price.
        _db.Offers.Remove(offer);
        await _db.SaveChangesAsync();
    }

    // Enforces "only one featured offer at a time" (see class header comment).
    private async Task UnfeatureAllAsync(int? exceptOfferId = null)
    {
        var others = await _db.Offers
            .Where(o => o.IsFeatured && o.Id != (exceptOfferId ?? -1))
            .ToListAsync();
        foreach (var other in others)
        {
            other.IsFeatured = false;
        }
    }

    private async Task ValidateOfferItemsAsync(List<OfferItemInputDto> items)
    {
        foreach (var item in items)
        {
            var exists = await _db.Products.AnyAsync(p => p.Id == item.ProductId);
            if (!exists)
            {
                throw ApiException.BadRequest($"Product #{item.ProductId} in the offer breakdown does not exist.");
            }
        }
    }

    // Maps the Offer entity (with its OfferItems loaded) to its wire shape.
    private static OfferDto ToDto(Offer o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Description = o.Description,
        OldPrice = o.OldPrice,
        NewPrice = o.NewPrice,
        IsActive = o.IsActive,
        IsFeatured = o.IsFeatured,
        Badge = o.Badge,
        Items = o.OfferItems
            .Select(oi => new OfferItemInputDto { ProductId = oi.ProductId, Quantity = oi.Quantity })
            .ToList(),
    };
}
