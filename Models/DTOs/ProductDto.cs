// FILE: Models/DTOs/ProductDto.cs
// PURPOSE: The wire shapes for the Product resource
//          (08-API-CONTRACT.md sections 1 and 3), verified field-for-field
//          against admin/js/products.js and customer/js/app.js.
// LOGIC:   OrderCount is nullable and only populated by the best-sellers
//          endpoints (GET /api/products/best-sellers and
//          GET /api/admin/reports/best-sellers) - every other endpoint that
//          returns a ProductDto leaves it null, which the frontend already
//          handles gracefully (`p.orderCount ?? "—"` in admin/js/dashboard.js).

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

// GET /api/products, GET /api/products/{id}, best-sellers response shape.
public class ProductDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }

    // Only set on the two best-sellers endpoints - see class header comment.
    public int? OrderCount { get; set; }
}

// Request body for POST /api/admin/products and PUT /api/admin/products/{id}.
// Deliberately has no Id or IsAvailable field - availability has its own
// dedicated PATCH endpoint (UpdateProductAvailabilityDto below), matching
// 08-API-CONTRACT.md exactly.
public class ProductUpsertDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal Price { get; set; }

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // security-test-report.md F6: MaxLength alone allowed any scheme
    // (javascript:, data:, ...) through to a field the frontend renders as
    // an <img src>. Requiring http(s) here is defense-in-depth alongside
    // the frontend's own matching check (customer/js/app.js safeImageUrl,
    // admin/js/categories.js safeImageUrl) - each side still protects
    // itself even if the other one is ever bypassed or edited separately.
    [Required, MaxLength(500), RegularExpression(@"^(?:/assets/menu/[a-zA-Z0-9_-]+[.]webp|https?://\S+)$",
        ErrorMessage = "ImageUrl must be an HTTP(S) URL or a bundled menu image path.")]
    public string ImageUrl { get; set; } = string.Empty;
}

// Request body for PATCH /api/admin/products/{id}/availability - the
// one-tap Sold Out toggle.
public class UpdateProductAvailabilityDto
{
    public bool IsAvailable { get; set; }
}
