// FILE: Data/AppDbContext.cs
// PURPOSE: The single class allowed to open a database connection
//          (04-BACKEND-ARCHITECTURE.md section 3 - "AppDbContext is the
//          single class allowed to open a database connection"). Every
//          Service in Services/ receives this through DI; Controllers never
//          see it directly.
// LOGIC:   Inherits IdentityDbContext<AppUser> instead of plain DbContext so
//          ASP.NET Core Identity's own tables (AspNetUsers, AspNetRoles,
//          AspNetUserRoles, ...) are created by the same migration as every
//          Phase 1 table, matching 03-DATABASE-SCHEMA.md section 3.
//
//          Delete-behavior choices below are deliberate, not EF Core's bare
//          defaults - each one matches an explicit statement in the docs:
//            - Product -> Category: Restrict. A category with products in
//              it cannot be deleted out from under them (CategoryService
//              also checks this itself first, for a friendlier error
//              message than a raw database exception).
//            - OrderItem -> Product / OrderItem -> Offer: SetNull. Matches
//              03-DATABASE-SCHEMA.md's OrderItem section precisely: "a
//              changed or deleted product/offer never corrupts a past order
//              or receipt" - ItemNameSnapshot/UnitPrice already preserve
//              everything a receipt needs.
//            - OfferItem -> Offer: Cascade. An OfferItem has no existence
//              independent of the Offer it belongs to (03-DATABASE-SCHEMA.md:
//              "what an offer is actually made of").
//            - OfferItem -> Product: Restrict. Silently dropping a product
//              from a combo's breakdown when it's deleted would quietly
//              change what a customer receives for that price - ProductService
//              blocks this with a friendly error instead (see ProductService.cs).
//            - Order -> RestaurantTable: Restrict. There is deliberately no
//              DELETE endpoint for tables in 08-API-CONTRACT.md - only
//              deactivation - so this is a safety net, not something the API
//              can trigger today.
//            - OrderItem -> Order: Cascade. Line items are owned by their order.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Models.Entities;

namespace ScanEat.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferItem> OfferItems => Set<OfferItem>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RestaurantInfo> RestaurantInfos => Set<RestaurantInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required first: wires up Identity's own AspNetUsers/AspNetRoles/... tables.
        base.OnModelCreating(modelBuilder);

        // ---- Category -------------------------------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.ImageUrl).IsRequired().HasMaxLength(500);
        });

        // ---- Product ----------------------------------------------------
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(10, 2);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Offer ------------------------------------------------------
        modelBuilder.Entity<Offer>(entity =>
        {
            entity.Property(o => o.Title).IsRequired().HasMaxLength(150);
            entity.Property(o => o.Description).HasMaxLength(1000);
            entity.Property(o => o.Badge).HasMaxLength(50);
            entity.Property(o => o.OldPrice).HasPrecision(10, 2);
            entity.Property(o => o.NewPrice).HasPrecision(10, 2);
        });

        // ---- OfferItem ----------------------------------------------------
        modelBuilder.Entity<OfferItem>(entity =>
        {
            entity.HasOne(oi => oi.Offer)
                  .WithMany(o => o.OfferItems)
                  .HasForeignKey(oi => oi.OfferId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OfferItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- RestaurantTable ----------------------------------------------
        modelBuilder.Entity<RestaurantTable>(entity =>
        {
            entity.Property(t => t.TableNumber).IsRequired().HasMaxLength(20);
            entity.Property(t => t.QrToken).IsRequired().HasMaxLength(100);
            // Every table's QR token must be unique - this is the whole
            // security property 06-QR-CODE-AND-TABLE-FLOW.md section 2 relies on.
            entity.HasIndex(t => t.QrToken).IsUnique();
        });

        // ---- Order ------------------------------------------------------
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Subtotal).HasPrecision(10, 2);
            entity.Property(o => o.Tax).HasPrecision(10, 2);
            entity.Property(o => o.Total).HasPrecision(10, 2);

            // Stored as text, not a number - see OrderStatus.cs header comment
            // and 03-DATABASE-SCHEMA.md section 5.
            entity.Property(o => o.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(o => o.Table)
                  .WithMany(t => t.Orders)
                  .HasForeignKey(o => o.TableId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- OrderItem ----------------------------------------------------
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.ItemNameSnapshot).IsRequired().HasMaxLength(150);
            entity.Property(oi => oi.UnitPrice).HasPrecision(10, 2);
            entity.Property(oi => oi.Notes).HasMaxLength(300);

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(oi => oi.Offer)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OfferId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- RestaurantInfo -------------------------------------------------
        modelBuilder.Entity<RestaurantInfo>(entity =>
        {
            entity.Property(r => r.AboutText).HasMaxLength(2000);
            entity.Property(r => r.OpeningHours).HasMaxLength(100);
            entity.Property(r => r.Certificates).HasMaxLength(2000);
        });
    }
}
