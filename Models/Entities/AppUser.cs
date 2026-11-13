// FILE: Models/Entities/AppUser.cs
// PURPOSE: The Identity user class for staff accounts (Admin / Kitchen-Cashier).
//          03-DATABASE-SCHEMA.md section 3: "Rather than a custom Users
//          table, staff accounts use ASP.NET Core Identity, which
//          auto-generates its own tables (AspNetUsers, AspNetRoles, ...)."
//          This class IS that AspNetUsers row - IdentityUser already has
//          everything Phase 1 needs (UserName, hashed PasswordHash, etc.),
//          so nothing is added on top of it. Two roles are used: "Admin"
//          and "Staff" (seeded in Data/SeedData.cs).

using Microsoft.AspNetCore.Identity;

namespace ScanEat.Api.Models.Entities;

public class AppUser : IdentityUser
{
}
