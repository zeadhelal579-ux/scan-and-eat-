// FILE: Models/DTOs/CategoryDto.cs
// PURPOSE: The wire shapes for the Category resource
//          (08-API-CONTRACT.md sections 1 and 3), verified field-for-field
//          against what the already-built frontend actually reads/sends
//          (admin/js/categories.js, customer/js/app.js renderCategories()).

using System.ComponentModel.DataAnnotations;

namespace ScanEat.Api.Models.DTOs;

// GET /api/categories response shape (also used inside admin/kitchen reads).
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

// Request body for POST /api/admin/categories and PUT /api/admin/categories/{id}.
public class CategoryUpsertDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // security-test-report.md F6 - see the matching comment in
    // Models/DTOs/ProductDto.cs's ProductUpsertDto.ImageUrl.
    [Required, MaxLength(500), RegularExpression(@"^(?:/assets/menu/[a-zA-Z0-9_-]+[.]webp|https?://\S+)$",
        ErrorMessage = "ImageUrl must be an HTTP(S) URL or a bundled menu image path.")]
    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
