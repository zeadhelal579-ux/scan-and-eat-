/*
  FILE: customer/js/demo-data.js
  PURPOSE: Local stand-in for the backend's catalog data (Categories,
           Products, Offers, RestaurantInfo), used ONLY when
           customer/js/api.js cannot reach the real API
           (GET /api/categories, /api/products, /api/offers,
           /api/restaurant-info - see 08-API-CONTRACT.md section 1).
  LOGIC:   - This is the *exact* 30-item / 6-category / 5-offer menu that
             used to be hardcoded directly in HTMLPage1.html, extracted
             programmatically (not retyped) and reshaped into the same
             CategoryDto / ProductDto / OfferDto / RestaurantInfoDto shapes
             03-DATABASE-SCHEMA.md and 08-API-CONTRACT.md describe, with
             sequential integer ids the way a real database would assign
             them (Category.Id, Product.Id, Offer.Id).
           - Two data bugs present in the original hardcoded HTML are fixed
             here (documented in the README's "Fixes to Pre-Existing Data"
             section):
               1. The old "Best Seller" highlight ("Chicken Ranch Supreme",
                  EGP 210) did not match any real product on the menu. It is
                  replaced by DEMO_BEST_SELLER_IDS, five real product ids -
                  matching how 03-DATABASE-SCHEMA.md says best-sellers must
                  work ("computed on demand ... not a flag").
               2. The old "Best Offer" highlight ("The Ultimate Combo", EGP
                  250) did not match any of the five real combo offers. It is
                  replaced by offer id 1 (Twin Burger Pack), which now
                  carries isFeatured: true - matching how
                  03-DATABASE-SCHEMA.md says the Offer.IsFeatured flag works.
           - offerDemoData[].items is a best-effort OfferItem breakdown
             (which real products + quantities each combo is made of),
             filled in only where the offer's own description names a
             product that exists in the catalog below. It's illustrative,
             not a claim about the restaurant's real combo composition.
*/

const demoAssetRoot = new URL("../../assets/menu/", document.currentScript.src);
const demoMenuImage = (name) => new URL(`${name}.webp`, demoAssetRoot).href;

const demoCategories = [
  { id: 1, name: "Pasta", imageUrl: demoMenuImage("1"), displayOrder: 1 },
  { id: 2, name: "Pizza", imageUrl: demoMenuImage("8"), displayOrder: 2 },
  { id: 3, name: "Burgers", imageUrl: demoMenuImage("11"), displayOrder: 3 },
  { id: 4, name: "Sushi", imageUrl: demoMenuImage("18"), displayOrder: 4 },
  { id: 5, name: "Sandwiches", imageUrl: demoMenuImage("21"), displayOrder: 5 },
  { id: 6, name: "Desserts", imageUrl: demoMenuImage("27"), displayOrder: 6 },
  { id: 7, name: "Starters", imageUrl: demoMenuImage("31"), displayOrder: 7 },
  { id: 8, name: "Drinks", imageUrl: demoMenuImage("34"), displayOrder: 8 }
];

const demoProducts = [
  { id: 1, categoryId: 1, name: "Chicken Alfredo Fettuccine", description: "Rich creamy white sauce, grilled chicken breast slices, fresh mushrooms.", price: 140, imageUrl: demoMenuImage("1"), isAvailable: true },
  { id: 2, categoryId: 1, name: "Spaghetti Bolognese", description: "Traditional slow-cooked Italian minced beef ragu, parmesan cheese.", price: 130, imageUrl: demoMenuImage("2"), isAvailable: true },
  { id: 3, categoryId: 1, name: "Penne Arrabbiata", description: "Spicy garlic tomato sauce, fresh basil leaves, premium olive oil.", price: 110, imageUrl: demoMenuImage("3"), isAvailable: true },
  { id: 4, categoryId: 1, name: "Creamy Basil Pesto", description: "Penne tossed in basil pesto with parmesan.", price: 150, imageUrl: demoMenuImage("4"), isAvailable: true },
  { id: 5, categoryId: 1, name: "Classic Baked Lasagna", description: "Layers of pasta sheet, savory beef filling, rich b\u00e9chamel, melted mozzarella.", price: 175, imageUrl: demoMenuImage("5"), isAvailable: true },
  { id: 6, categoryId: 2, name: "Chicken Ranch Pizza", description: "Grilled chicken cubes, signature ranch, mozzarella, tomatoes, jalapenos.", price: 180, imageUrl: demoMenuImage("6"), isAvailable: true },
  { id: 7, categoryId: 2, name: "Margherita Classic", description: "Authentic Italian tomato sauce, fresh buffalo mozzarella, fresh basil, olive oil.", price: 120, imageUrl: demoMenuImage("7"), isAvailable: true },
  { id: 8, categoryId: 2, name: "Beef Pepperoni Feast", description: "Loaded premium beef pepperoni slices, extra mozzarella cheese, oregano.", price: 165, imageUrl: demoMenuImage("8"), isAvailable: true },
  { id: 9, categoryId: 2, name: "Smoky BBQ Chicken", description: "Barbecue chicken strips, red onions, bell peppers, sweet smoky glaze.", price: 175, imageUrl: demoMenuImage("9"), isAvailable: true },
  { id: 10, categoryId: 2, name: "Garden Vegetarian", description: "Mushrooms, colorful bell peppers, sweet corn, black olives, onions.", price: 135, imageUrl: demoMenuImage("10"), isAvailable: true },
  { id: 11, categoryId: 3, name: "Classic Cheeseburger", description: "Grilled beef, melted cheddar, lettuce, tomato and red onion.", price: 190, imageUrl: demoMenuImage("11"), isAvailable: true },
  { id: 12, categoryId: 3, name: "Mushroom Swiss Angus", description: "Sauteed fresh mushroom mix, melted creamy Swiss cheese, truffle mayo.", price: 170, imageUrl: demoMenuImage("12"), isAvailable: true },
  { id: 13, categoryId: 3, name: "Spicy Crispy Zinger", description: "Fiery deep-fried chicken breast, spicy sauce, pickles, shredded lettuce.", price: 145, imageUrl: demoMenuImage("13"), isAvailable: true },
  { id: 14, categoryId: 3, name: "Texas BBQ Beef Bacon", description: "Beef bacon slices, crispy fried onion rings, sweet BBQ glaze, cheddar.", price: 185, imageUrl: demoMenuImage("14"), isAvailable: true },
  { id: 15, categoryId: 3, name: "Monster Double Smash", description: "Two smashed crispy beef patties, double cheddar layers, caramelized onions.", price: 210, imageUrl: demoMenuImage("15"), isAvailable: true },
  { id: 16, categoryId: 4, name: "California Roll (4 Pcs)", description: "Crab sticks, avocado, crisp cucumber, coated with flying fish roe.", price: 120, imageUrl: demoMenuImage("16"), isAvailable: true },
  { id: 17, categoryId: 4, name: "Spicy Tuna Crunch (4 Pcs)", description: "Fresh yellowfin tuna, spicy sriracha mayo mix, tempura flakes.", price: 145, imageUrl: demoMenuImage("17"), isAvailable: true },
  { id: 18, categoryId: 4, name: "Golden Dragon Roll (4 Pcs)", description: "Crispy shrimp tempura, wrapped in unagi eel slices and fresh avocado.", price: 160, imageUrl: demoMenuImage("18"), isAvailable: true },
  { id: 19, categoryId: 4, name: "Salmon Nigiri (3 Pcs)", description: "Hand-pressed vinegared rice topped with fresh raw salmon slices.", price: 95, imageUrl: demoMenuImage("19"), isAvailable: true },
  { id: 20, categoryId: 4, name: "Philadelphia Roll (4 Pcs)", description: "Fresh salmon, velvety cream cheese layer, crisp avocado elements.", price: 140, imageUrl: demoMenuImage("20"), isAvailable: true },
  { id: 21, categoryId: 5, name: "Classic Triple Club", description: "Toasted bread layers, smoked turkey, grilled chicken, egg, cheddar, mayo.", price: 115, imageUrl: demoMenuImage("21"), isAvailable: true },
  { id: 22, categoryId: 5, name: "Philly Ribeye Cheesesteak", description: "Saut\u00e9ed ribeye steak strips, caramelized peppers and onions, melted provolone.", price: 165, imageUrl: demoMenuImage("22"), isAvailable: true },
  { id: 23, categoryId: 5, name: "Tuscan Caprese Panini", description: "Fresh mozzarella, vine tomatoes, aromatic basil pesto sauce, pressed ciabatta.", price: 90, imageUrl: demoMenuImage("23"), isAvailable: true },
  { id: 24, categoryId: 5, name: "Smoked Turkey & Emmental", description: "Premium cold smoked turkey slices, matured Emmental cheese, dijon mustard hint.", price: 105, imageUrl: demoMenuImage("24"), isAvailable: true },
  { id: 25, categoryId: 5, name: "Spicy Jalapeno Tuna Melt", description: "Flaked white tuna mix, spicy jalapenos, melted gouda cheese, toasted sourdough.", price: 95, imageUrl: demoMenuImage("25"), isAvailable: true },
  { id: 26, categoryId: 6, name: "Chocolate Molten Lava Cake", description: "Warm dark chocolate cake with a molten center and fresh berries.", price: 85, imageUrl: demoMenuImage("26"), isAvailable: true },
  { id: 27, categoryId: 6, name: "New York Strawberry Cheesecake", description: "Classic dense cheesecake base topped with rich sweet strawberry compote.", price: 90, imageUrl: demoMenuImage("27"), isAvailable: true },
  { id: 28, categoryId: 6, name: "Italian Espresso Tiramisu", description: "Ladyfingers soaked in espresso coffee, layered with whipped mascarpone cream.", price: 95, imageUrl: demoMenuImage("28"), isAvailable: true },
  { id: 29, categoryId: 6, name: "Red Velvet Pastry Layer", description: "Fluffy red velvet cake layers with sweet traditional cream cheese frosting.", price: 80, imageUrl: demoMenuImage("29"), isAvailable: true },
  { id: 30, categoryId: 6, name: "Belgian Chocolate Waffles", description: "Fresh golden baked waffles smothered with premium milk and white Belgian chocolate.", price: 110, imageUrl: demoMenuImage("30"), isAvailable: true },
  { id: 31, categoryId: 7, name: "Sea Salt Fries", description: "Golden skin-on fries tossed with sea salt.", price: 65, imageUrl: demoMenuImage("31"), isAvailable: true },
  { id: 32, categoryId: 7, name: "Crispy Chicken Bites", description: "Buttermilk chicken bites with smoky honey dip.", price: 115, imageUrl: demoMenuImage("32"), isAvailable: true },
  { id: 33, categoryId: 7, name: "House Caesar Salad", description: "Romaine, shaved parmesan and toasted croutons.", price: 95, imageUrl: demoMenuImage("33"), isAvailable: true },
  { id: 34, categoryId: 8, name: "Mint Lemonade", description: "Fresh lemon, crushed mint and plenty of ice.", price: 70, imageUrl: demoMenuImage("34"), isAvailable: true },
  { id: 35, categoryId: 8, name: "Iced Latte", description: "Double espresso over milk and ice.", price: 85, imageUrl: demoMenuImage("35"), isAvailable: true },
  { id: 36, categoryId: 8, name: "Espresso", description: "A short double shot of our house roast.", price: 45, imageUrl: demoMenuImage("36"), isAvailable: true },
  { id: 37, categoryId: 8, name: "Fresh Orange Juice", description: "Orange juice pressed to order.", price: 75, imageUrl: demoMenuImage("37"), isAvailable: true },
  { id: 38, categoryId: 6, name: "Vanilla Ice Cream", description: "Two scoops of vanilla bean ice cream.", price: 70, imageUrl: demoMenuImage("38"), isAvailable: true }
];

const demoOffers = [
  { id: 1, title: "Twin Burger Pack", description: "Get 2 Classic Cheeseburgers with sea salt fries and two mint lemonades.", oldPrice: 585, newPrice: 495, isActive: true, isFeatured: true, badge: "Burger Combo", items: [{ productId: 11, quantity: 2 }, { productId: 31, quantity: 1 }, { productId: 34, quantity: 2 }] },
  { id: 2, title: "Double Large Feast", description: "A Margherita Classic and a Chicken Ranch Pizza.", oldPrice: 300, newPrice: 260, isActive: true, isFeatured: false, badge: "Pizza Night", items: [{ productId: 7, quantity: 1 }, { productId: 6, quantity: 1 }] },
  { id: 3, title: "Tokyo Samurai Platter", description: "Enjoy a diverse 12-piece premium roll selection (California, Dragon, and Spicy Tuna rolls).", oldPrice: 425, newPrice: 390, isActive: true, isFeatured: false, badge: "Sushi Deal", items: [{ productId: 16, quantity: 1 }, { productId: 18, quantity: 1 }, { productId: 17, quantity: 1 }] },
  { id: 4, title: "Italian Romance Duo", description: "1 Chicken Alfredo Pasta + 1 Spaghetti Bolognese + 2 House Caesar Salads.", oldPrice: 460, newPrice: 395, isActive: true, isFeatured: false, badge: "Pasta Combo", items: [{ productId: 1, quantity: 1 }, { productId: 2, quantity: 1 }, { productId: 33, quantity: 2 }] },
  { id: 5, title: "Mega Dessert Carnival", description: "1 Molten Lava Cake + 1 Strawberry Cheesecake + 2 Hot Espresso Cups to complete your meal.", oldPrice: 265, newPrice: 225, isActive: true, isFeatured: false, badge: "Sweet Deal", items: [{ productId: 26, quantity: 1 }, { productId: 27, quantity: 1 }, { productId: 36, quantity: 2 }] }
];

const DEMO_BEST_SELLER_IDS = [6, 15, 18, 1, 26];

const demoRestaurantInfo = {
  aboutText: "Burgers off the grill, pizza from the oven and coffee pulled to order. Open daily.",
  openingHours: "9:00 AM – midnight",
  certificates: [],
  establishedYear: 2022
};

// Mirrors GET /api/products/best-sellers?days=&count= against the local
// catalog above (see the header comment for why these five ids were chosen).
function demoBestSellers({ count = 5 } = {}) {
    return DEMO_BEST_SELLER_IDS.slice(0, count)
        .map((id) => DemoStore.read("products", demoProducts).find((p) => p.id === id && p.isAvailable))
        .filter(Boolean);
}

// Mirrors GET /api/products?category=&search=
function demoProductList({ category, search } = {}) {
    const term = (search || "").toLowerCase().trim();
    return DemoStore.read("products", demoProducts).filter((p) => {
        // Number(category) so this matches whether the caller passes a
        // numeric id or a query-string-style string ("2") - the real
        // ProductsController does the equivalent int.TryParse server-side,
        // so this stub should accept the same shapes it does.
        const matchesCategory = !category || category === "all" || p.categoryId === Number(category);
        const haystack = (p.name + " " + p.description).toLowerCase();
        const matchesSearch = !term || haystack.includes(term);
        return matchesCategory && matchesSearch;
    });
}
