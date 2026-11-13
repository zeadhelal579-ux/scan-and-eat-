// FILE: Models/DTOs/OfferDto.cs
// PURPOSE: The wire shapes for the Offer resource
//          (08-API-CONTRACT.md sections 1 and 3), verified field-for-field
//          against admin/js/offers.js and customer/js/app.js (renderOffers,
//          renderHighlights).
// LOGIC:   Items is an OPTIONAL breakdown (product id + quantity, matching
//          OfferItem) - 08-API-CONTRACT.md's contract shows offer writes as
//          "OfferDto (incl. product list)", but the currently-built admin
//          Offers page has no UI for it yet, so it's nullable/omittable
//          here and simply left empty when not sent - matching the
//          already-shipped frontend's actual behavior without blocking the
//          documented future UI for it.

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

// GET /api/offers response shape.
public class OfferDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? Badge { get; set; }
    public List<OfferItemInputDto> Items { get; set; } = new();
}

// Request body for POST /api/admin/offers and PUT /api/admin/offers/{id}.
public class OfferUpsertDto
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "OldPrice cannot be negative.")]
    public decimal OldPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "NewPrice cannot be negative.")]
    public decimal NewPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    [MaxLength(50)]
    public string? Badge { get; set; }

    // Optional - see class header comment. Null/omitted means "leave the breakdown unchanged" on update, or "no breakdown" on create.
    public List<OfferItemInputDto>? Items { get; set; }
}

// One line of an offer's product breakdown (mirrors the OfferItem entity).
public class OfferItemInputDto
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;
}
