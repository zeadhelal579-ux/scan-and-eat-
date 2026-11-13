// FILE: Models/Entities/Category.cs
// PURPOSE: Mirrors the "Category" table from 03-DATABASE-SCHEMA.md section 2.
//          Products are grouped into categories (Pasta, Pizza, Burger...).
//          Kept as its own table - not a text label on Product - because the
//          frontend treats categories as visual objects (their own image)
//          with an admin-controlled display order.
// LOGIC:   DisplayOrder controls left-to-right order in the customer app's
//          horizontal-scroll category row (customer/js/app.js renderCategories()).

namespace ScanEat.Api.Models.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    // Navigation: every product that belongs to this category.
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
