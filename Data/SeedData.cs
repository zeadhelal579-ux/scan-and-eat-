// FILE: Data/SeedData.cs
// PURPOSE: Idempotent startup seeding (called once from Program.cs after
//          migrations run). Two jobs:
//            1. Identity roles ("Admin", "Staff") + two demo staff accounts,
//               so the app is loggable-into the moment it starts, since
//               08-API-CONTRACT.md has no self-registration endpoint.
//            2. The exact same 6 categories / 30 products / 5 offers /
//               restaurant info the already-built frontend's demo mode
//               shows (customer/js/demo-data.js, admin/js/admin-demo-data.js) -
//               so the instant this real backend is reachable, every app
//               shows the SAME familiar catalog instead of an empty menu.
//               An empty database would actually be a worse experience than
//               what the frontend already has today: the frontend only
//               falls back to demo data when the API is UNREACHABLE, not
//               when it responds with an empty list - a live-but-empty
//               backend would render an empty menu instead. Seeding closes
//               that gap.
// LOGIC:   Every step checks "does this already exist?" first, so running
//          this on every startup (as Program.cs does) is always safe and
//          never duplicates rows on a second run.

using Microsoft.AspNetCore.Identity;
using ScanEat.Api.Models.Entities;
using ScanEat.Api.Models.Enums;

namespace ScanEat.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();

        await SeedRolesAndDemoAccountsAsync(services, configuration, logger);
        await SeedCatalogAsync(db, logger);
    }

    // ---- Identity: roles + demo staff accounts -----------------------------
    private static async Task SeedRolesAndDemoAccountsAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        foreach (var roleName in new[] { "Admin", "Staff" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // See appsettings.json "SeedDemoAccounts" (security-test-report.md
        // F5): defaults to false in the base appsettings.json and true only
        // in appsettings.Development.json, so a real deployment doesn't get
        // a known admin/kitchen account unless someone opts in on purpose.
        // The "false" literal below is a second safety net, not the primary
        // control - it only matters if that config key is missing entirely.
        if (!configuration.GetValue("SeedDemoAccounts", false))
        {
            return;
        }

        // security-test-report.md F13: RequiredLength=12 + RequireUppercase
        // (Program.cs) means the old "demo123" no longer satisfies the
        // policy - CreateAsync below would reject it. Keep this value in
        // sync with frontend_src/kitchen/js/login-init.js and
        // frontend_src/admin/js/login-init.js's demoUsers entries, and with
        // both README.md files, if you ever change it again.
        const string demoPassword = "ScanEatDemo2026";
        await EnsureStaffUserAsync(userManager, "admin", demoPassword, "Admin", logger);
        await EnsureStaffUserAsync(userManager, "kitchen", demoPassword, "Staff", logger);
    }

    private static async Task EnsureStaffUserAsync(UserManager<AppUser> userManager, string username, string password, string role, ILogger logger)
    {
        var existing = await userManager.FindByNameAsync(username);
        if (existing is not null)
        {
            return;
        }

        var user = new AppUser { UserName = username };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Could not seed demo account '{Username}': {Errors}", username, errors);
            return;
        }

        await userManager.AddToRoleAsync(user, role);
    }

    // ---- Catalog: categories, products, offers, restaurant info, tables ----
    private static async Task SeedCatalogAsync(AppDbContext db, ILogger logger)
    {
        if (!db.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Pasta", ImageUrl = "/assets/menu/1.webp", DisplayOrder = 1 },
                new() { Name = "Pizza", ImageUrl = "/assets/menu/8.webp", DisplayOrder = 2 },
                new() { Name = "Burgers", ImageUrl = "/assets/menu/11.webp", DisplayOrder = 3 },
                new() { Name = "Sushi", ImageUrl = "/assets/menu/18.webp", DisplayOrder = 4 },
                new() { Name = "Sandwiches", ImageUrl = "/assets/menu/21.webp", DisplayOrder = 5 },
                new() { Name = "Desserts", ImageUrl = "/assets/menu/27.webp", DisplayOrder = 6 },
                new() { Name = "Starters", ImageUrl = "/assets/menu/31.webp", DisplayOrder = 7 },
                new() { Name = "Drinks", ImageUrl = "/assets/menu/34.webp", DisplayOrder = 8 },
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            var catId = categories.ToDictionary(c => c.Name, c => c.Id);

            var products = new List<Product>
            {
                new() { CategoryId = catId["Pasta"], Name = "Chicken Alfredo Fettuccine", Description = "Rich creamy white sauce, grilled chicken breast slices, fresh mushrooms.", Price = 140, ImageUrl = "/assets/menu/1.webp" },
                new() { CategoryId = catId["Pasta"], Name = "Spaghetti Bolognese", Description = "Traditional slow-cooked Italian minced beef ragu, parmesan cheese.", Price = 130, ImageUrl = "/assets/menu/2.webp" },
                new() { CategoryId = catId["Pasta"], Name = "Penne Arrabbiata", Description = "Spicy garlic tomato sauce, fresh basil leaves, premium olive oil.", Price = 110, ImageUrl = "/assets/menu/3.webp" },
                new() { CategoryId = catId["Pasta"], Name = "Creamy Basil Pesto", Description = "Penne tossed in basil pesto with parmesan.", Price = 150, ImageUrl = "/assets/menu/4.webp" },
                new() { CategoryId = catId["Pasta"], Name = "Classic Baked Lasagna", Description = "Layers of pasta sheet, savory beef filling, rich b\u00e9chamel, melted mozzarella.", Price = 175, ImageUrl = "/assets/menu/5.webp" },
                new() { CategoryId = catId["Pizza"], Name = "Chicken Ranch Pizza", Description = "Grilled chicken cubes, signature ranch, mozzarella, tomatoes, jalapenos.", Price = 180, ImageUrl = "/assets/menu/6.webp" },
                new() { CategoryId = catId["Pizza"], Name = "Margherita Classic", Description = "Authentic Italian tomato sauce, fresh buffalo mozzarella, fresh basil, olive oil.", Price = 120, ImageUrl = "/assets/menu/7.webp" },
                new() { CategoryId = catId["Pizza"], Name = "Beef Pepperoni Feast", Description = "Loaded premium beef pepperoni slices, extra mozzarella cheese, oregano.", Price = 165, ImageUrl = "/assets/menu/8.webp" },
                new() { CategoryId = catId["Pizza"], Name = "Smoky BBQ Chicken", Description = "Barbecue chicken strips, red onions, bell peppers, sweet smoky glaze.", Price = 175, ImageUrl = "/assets/menu/9.webp" },
                new() { CategoryId = catId["Pizza"], Name = "Garden Vegetarian", Description = "Mushrooms, colorful bell peppers, sweet corn, black olives, onions.", Price = 135, ImageUrl = "/assets/menu/10.webp" },
                new() { CategoryId = catId["Burgers"], Name = "Classic Cheeseburger", Description = "Grilled beef, melted cheddar, lettuce, tomato and red onion.", Price = 190, ImageUrl = "/assets/menu/11.webp" },
                new() { CategoryId = catId["Burgers"], Name = "Mushroom Swiss Angus", Description = "Sauteed fresh mushroom mix, melted creamy Swiss cheese, truffle mayo.", Price = 170, ImageUrl = "/assets/menu/12.webp" },
                new() { CategoryId = catId["Burgers"], Name = "Spicy Crispy Zinger", Description = "Fiery deep-fried chicken breast, spicy sauce, pickles, shredded lettuce.", Price = 145, ImageUrl = "/assets/menu/13.webp" },
                new() { CategoryId = catId["Burgers"], Name = "Texas BBQ Beef Bacon", Description = "Beef bacon slices, crispy fried onion rings, sweet BBQ glaze, cheddar.", Price = 185, ImageUrl = "/assets/menu/14.webp" },
                new() { CategoryId = catId["Burgers"], Name = "Monster Double Smash", Description = "Two smashed crispy beef patties, double cheddar layers, caramelized onions.", Price = 210, ImageUrl = "/assets/menu/15.webp" },
                new() { CategoryId = catId["Sushi"], Name = "California Roll (4 Pcs)", Description = "Crab sticks, avocado, crisp cucumber, coated with flying fish roe.", Price = 120, ImageUrl = "/assets/menu/16.webp" },
                new() { CategoryId = catId["Sushi"], Name = "Spicy Tuna Crunch (4 Pcs)", Description = "Fresh yellowfin tuna, spicy sriracha mayo mix, tempura flakes.", Price = 145, ImageUrl = "/assets/menu/17.webp" },
                new() { CategoryId = catId["Sushi"], Name = "Golden Dragon Roll (4 Pcs)", Description = "Crispy shrimp tempura, wrapped in unagi eel slices and fresh avocado.", Price = 160, ImageUrl = "/assets/menu/18.webp" },
                new() { CategoryId = catId["Sushi"], Name = "Salmon Nigiri (3 Pcs)", Description = "Hand-pressed vinegared rice topped with fresh raw salmon slices.", Price = 95, ImageUrl = "/assets/menu/19.webp" },
                new() { CategoryId = catId["Sushi"], Name = "Philadelphia Roll (4 Pcs)", Description = "Fresh salmon, velvety cream cheese layer, crisp avocado elements.", Price = 140, ImageUrl = "/assets/menu/20.webp" },
                new() { CategoryId = catId["Sandwiches"], Name = "Classic Triple Club", Description = "Toasted bread layers, smoked turkey, grilled chicken, egg, cheddar, mayo.", Price = 115, ImageUrl = "/assets/menu/21.webp" },
                new() { CategoryId = catId["Sandwiches"], Name = "Philly Ribeye Cheesesteak", Description = "Saut\u00e9ed ribeye steak strips, caramelized peppers and onions, melted provolone.", Price = 165, ImageUrl = "/assets/menu/22.webp" },
                new() { CategoryId = catId["Sandwiches"], Name = "Tuscan Caprese Panini", Description = "Fresh mozzarella, vine tomatoes, aromatic basil pesto sauce, pressed ciabatta.", Price = 90, ImageUrl = "/assets/menu/23.webp" },
                new() { CategoryId = catId["Sandwiches"], Name = "Smoked Turkey & Emmental", Description = "Premium cold smoked turkey slices, matured Emmental cheese, dijon mustard hint.", Price = 105, ImageUrl = "/assets/menu/24.webp" },
                new() { CategoryId = catId["Sandwiches"], Name = "Spicy Jalapeno Tuna Melt", Description = "Flaked white tuna mix, spicy jalapenos, melted gouda cheese, toasted sourdough.", Price = 95, ImageUrl = "/assets/menu/25.webp" },
                new() { CategoryId = catId["Desserts"], Name = "Chocolate Molten Lava Cake", Description = "Warm dark chocolate cake with a molten center and fresh berries.", Price = 85, ImageUrl = "/assets/menu/26.webp" },
                new() { CategoryId = catId["Desserts"], Name = "New York Strawberry Cheesecake", Description = "Classic dense cheesecake base topped with rich sweet strawberry compote.", Price = 90, ImageUrl = "/assets/menu/27.webp" },
                new() { CategoryId = catId["Desserts"], Name = "Italian Espresso Tiramisu", Description = "Ladyfingers soaked in espresso coffee, layered with whipped mascarpone cream.", Price = 95, ImageUrl = "/assets/menu/28.webp" },
                new() { CategoryId = catId["Desserts"], Name = "Red Velvet Pastry Layer", Description = "Fluffy red velvet cake layers with sweet traditional cream cheese frosting.", Price = 80, ImageUrl = "/assets/menu/29.webp" },
                new() { CategoryId = catId["Desserts"], Name = "Belgian Chocolate Waffles", Description = "Fresh golden baked waffles smothered with premium milk and white Belgian chocolate.", Price = 110, ImageUrl = "/assets/menu/30.webp" },
                new() { CategoryId = catId["Starters"], Name = "Sea Salt Fries", Description = "Golden skin-on fries tossed with sea salt.", Price = 65, ImageUrl = "/assets/menu/31.webp" },
                new() { CategoryId = catId["Starters"], Name = "Crispy Chicken Bites", Description = "Buttermilk chicken bites with smoky honey dip.", Price = 115, ImageUrl = "/assets/menu/32.webp" },
                new() { CategoryId = catId["Starters"], Name = "House Caesar Salad", Description = "Romaine, shaved parmesan and toasted croutons.", Price = 95, ImageUrl = "/assets/menu/33.webp" },
                new() { CategoryId = catId["Drinks"], Name = "Mint Lemonade", Description = "Fresh lemon, crushed mint and plenty of ice.", Price = 70, ImageUrl = "/assets/menu/34.webp" },
                new() { CategoryId = catId["Drinks"], Name = "Iced Latte", Description = "Double espresso over milk and ice.", Price = 85, ImageUrl = "/assets/menu/35.webp" },
                new() { CategoryId = catId["Drinks"], Name = "Espresso", Description = "A short double shot of our house roast.", Price = 45, ImageUrl = "/assets/menu/36.webp" },
                new() { CategoryId = catId["Drinks"], Name = "Fresh Orange Juice", Description = "Orange juice pressed to order.", Price = 75, ImageUrl = "/assets/menu/37.webp" },
                new() { CategoryId = catId["Desserts"], Name = "Vanilla Ice Cream", Description = "Two scoops of vanilla bean ice cream.", Price = 70, ImageUrl = "/assets/menu/38.webp" },
            };
            db.Products.AddRange(products);
            await db.SaveChangesAsync();

            var productId = products.ToDictionary(p => p.Name, p => p.Id);

            var offers = new List<Offer>
            {
                new() { Title = "Twin Burger Pack", Description = "Get 2 Classic Cheeseburgers with sea salt fries and two mint lemonades.", OldPrice = 585, NewPrice = 495, IsActive = true, IsFeatured = true, Badge = "Burger Combo", OfferItems = [new() { ProductId = productId["Classic Cheeseburger"], Quantity = 2 }, new() { ProductId = productId["Sea Salt Fries"], Quantity = 1 }, new() { ProductId = productId["Mint Lemonade"], Quantity = 2 }] },
                new() { Title = "Double Large Feast", Description = "A Margherita Classic and a Chicken Ranch Pizza.", OldPrice = 300, NewPrice = 260, IsActive = true, IsFeatured = false, Badge = "Pizza Night", OfferItems = [new() { ProductId = productId["Margherita Classic"], Quantity = 1 }, new() { ProductId = productId["Chicken Ranch Pizza"], Quantity = 1 }] },
                new() { Title = "Tokyo Samurai Platter", Description = "Enjoy a diverse 12-piece premium roll selection (California, Dragon, and Spicy Tuna rolls).", OldPrice = 425, NewPrice = 390, IsActive = true, IsFeatured = false, Badge = "Sushi Deal", OfferItems = [new() { ProductId = productId["California Roll (4 Pcs)"], Quantity = 1 }, new() { ProductId = productId["Golden Dragon Roll (4 Pcs)"], Quantity = 1 }, new() { ProductId = productId["Spicy Tuna Crunch (4 Pcs)"], Quantity = 1 }] },
                new() { Title = "Italian Romance Duo", Description = "1 Chicken Alfredo Pasta + 1 Spaghetti Bolognese + 2 House Caesar Salads.", OldPrice = 460, NewPrice = 395, IsActive = true, IsFeatured = false, Badge = "Pasta Combo", OfferItems = [new() { ProductId = productId["Chicken Alfredo Fettuccine"], Quantity = 1 }, new() { ProductId = productId["Spaghetti Bolognese"], Quantity = 1 }, new() { ProductId = productId["House Caesar Salad"], Quantity = 2 }] },
                new() { Title = "Mega Dessert Carnival", Description = "1 Molten Lava Cake + 1 Strawberry Cheesecake + 2 Hot Espresso Cups to complete your meal.", OldPrice = 265, NewPrice = 225, IsActive = true, IsFeatured = false, Badge = "Sweet Deal", OfferItems = [new() { ProductId = productId["Chocolate Molten Lava Cake"], Quantity = 1 }, new() { ProductId = productId["New York Strawberry Cheesecake"], Quantity = 1 }, new() { ProductId = productId["Espresso"], Quantity = 2 }] },
            };
            db.Offers.AddRange(offers);
            await db.SaveChangesAsync();
        }

        if (!db.RestaurantInfos.Any())
        {
            db.RestaurantInfos.Add(new RestaurantInfo
            {
                AboutText = "We craft an exceptional culinary journey combining pristine ingredients with modern dining convenience. Located in the heart of the city, welcoming you daily from 9:00 AM to 12:00 AM.",
                OpeningHours = "9:00 AM - 12:00 AM",
                Certificates = string.Empty,
                EstablishedYear = 2022,
            });
            await db.SaveChangesAsync();
        }

        if (!db.RestaurantTables.Any())
        {
            var tables = new List<RestaurantTable>
            {
                new() { TableNumber = "01", QrToken = Guid.NewGuid().ToString(), IsActive = true },
                new() { TableNumber = "02", QrToken = Guid.NewGuid().ToString(), IsActive = true },
                new() { TableNumber = "03", QrToken = Guid.NewGuid().ToString(), IsActive = true },
                new() { TableNumber = "04", QrToken = Guid.NewGuid().ToString(), IsActive = true },
                new() { TableNumber = "05 (patio)", QrToken = Guid.NewGuid().ToString(), IsActive = true },
            };
            db.RestaurantTables.AddRange(tables);
            await db.SaveChangesAsync();

            // Printed once at startup purely as a developer convenience, so a
            // real end-to-end test (scan/open a link -> order -> see it on
            // the Kitchen Display) can be run immediately without first
            // opening the Admin Dashboard to look up a token.
            logger.LogInformation("Seeded {Count} tables. Example customer link for Table {TableNumber}: /customer/index.html?table={Token}",
                tables.Count, tables[0].TableNumber, tables[0].QrToken);
        }
    }
}
