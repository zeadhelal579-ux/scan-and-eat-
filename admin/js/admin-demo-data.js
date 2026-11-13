// Mirrors the catalog and table admin endpoints in the browser demo.
let demoAdminCategories = DemoStore.read("categories", demoCategories);
let demoAdminProducts = DemoStore.read("products", demoProducts);
let demoAdminOffers = DemoStore.read("offers", demoOffers);
let demoAdminTables = DemoStore.tables();

function nextId(rows) {
    return Math.max(0, ...rows.map((row) => row.id)) + 1;
}

/* ---------------------------------------------------------------------- *
 * Categories admin mirror (POST/PUT/DELETE /api/admin/categories)
 * ---------------------------------------------------------------------- */
function demoCreateCategory(dto) {
    demoAdminCategories = DemoStore.read("categories", demoCategories);
    const category = { id: nextId(demoAdminCategories), displayOrder: demoAdminCategories.length + 1, ...dto };
    demoAdminCategories.push(category);
    DemoStore.write("categories", demoAdminCategories);
    return category;
}
function demoUpdateCategory(id, dto) {
    demoAdminCategories = DemoStore.read("categories", demoCategories);
    const idx = demoAdminCategories.findIndex((c) => c.id === id);
    if (idx === -1) throw new Error("Category not found");
    demoAdminCategories[idx] = { ...demoAdminCategories[idx], ...dto };
    DemoStore.write("categories", demoAdminCategories);
    return demoAdminCategories[idx];
}
function demoDeleteCategory(id) {
    demoAdminCategories = DemoStore.read("categories", demoCategories);
    if (DemoStore.read("products", demoProducts).some((product) => product.categoryId === id)) {
        throw new Error("Move or remove the products in this category first.");
    }
    demoAdminCategories = demoAdminCategories.filter((c) => c.id !== id);
    DemoStore.write("categories", demoAdminCategories);
}

/* ---------------------------------------------------------------------- *
 * Products admin mirror (POST/PUT/PATCH/DELETE /api/admin/products)
 * ---------------------------------------------------------------------- */
function demoCreateProduct(dto) {
    demoAdminProducts = DemoStore.read("products", demoProducts);
    const product = { id: nextId(demoAdminProducts), isAvailable: true, ...dto };
    demoAdminProducts.push(product);
    DemoStore.write("products", demoAdminProducts);
    return product;
}
function demoUpdateProduct(id, dto) {
    demoAdminProducts = DemoStore.read("products", demoProducts);
    const idx = demoAdminProducts.findIndex((p) => p.id === id);
    if (idx === -1) throw new Error("Product not found");
    demoAdminProducts[idx] = { ...demoAdminProducts[idx], ...dto };
    DemoStore.write("products", demoAdminProducts);
    return demoAdminProducts[idx];
}
function demoSetProductAvailability(id, isAvailable) {
    demoAdminProducts = DemoStore.read("products", demoProducts);
    const product = demoAdminProducts.find((p) => p.id === id);
    if (!product) throw new Error("Product not found");
    product.isAvailable = isAvailable;
    DemoStore.write("products", demoAdminProducts);
    return product;
}
function demoDeleteProduct(id) {
    demoAdminProducts = DemoStore.read("products", demoProducts);
    if (DemoStore.read("offers", demoOffers).some((offer) => offer.items?.some(({ productId }) => productId === id))) {
        throw new Error("Remove this product from its offers first.");
    }
    demoAdminProducts = demoAdminProducts.filter((p) => p.id !== id);
    DemoStore.write("products", demoAdminProducts);
}

/* ---------------------------------------------------------------------- *
 * Offers admin mirror (POST/PUT/DELETE /api/admin/offers)
 * ---------------------------------------------------------------------- */
function demoCreateOffer(dto) {
    demoAdminOffers = DemoStore.read("offers", demoOffers);
    const offer = { id: nextId(demoAdminOffers), isActive: true, isFeatured: false, items: [], ...dto };
    if (dto.isFeatured) demoAdminOffers.forEach((entry) => { entry.isFeatured = false; });
    demoAdminOffers.push(offer);
    DemoStore.write("offers", demoAdminOffers);
    return offer;
}
function demoUpdateOffer(id, dto) {
    demoAdminOffers = DemoStore.read("offers", demoOffers);
    const idx = demoAdminOffers.findIndex((o) => o.id === id);
    if (idx === -1) throw new Error("Offer not found");
    if (dto.isFeatured) {
        // Only one offer can be featured at a time, matching
        // 03-DATABASE-SCHEMA.md's Offer.IsFeatured ("marks THE ONE offer").
        demoAdminOffers.forEach((o) => (o.isFeatured = false));
    }
    demoAdminOffers[idx] = { ...demoAdminOffers[idx], ...dto };
    DemoStore.write("offers", demoAdminOffers);
    return demoAdminOffers[idx];
}
function demoDeleteOffer(id) {
    demoAdminOffers = DemoStore.read("offers", demoOffers);
    demoAdminOffers = demoAdminOffers.filter((o) => o.id !== id);
    DemoStore.write("offers", demoAdminOffers);
}

/* ---------------------------------------------------------------------- *
 * Tables admin mirror (GET/POST/PATCH /api/admin/tables, QR code)
 * ---------------------------------------------------------------------- */
function demoListTables() {
    // Returns a COPY, not the live array - a real GET response is never a
    // live reference to server-side state either, and callers (tables.js)
    // independently push newly-created rows onto their own local list, so
    // handing out the live reference here would let both places grow the
    // same underlying array and silently double-count entries (a real bug
    // caught during testing - see the README's "Bugs Found & Fixed"
    // section for the exact symptom this caused).
    return DemoStore.tables();
}
function demoCreateTable(tableNumber) {
    demoAdminTables = DemoStore.tables();
    if (demoAdminTables.some((table) => table.tableNumber.toLowerCase() === tableNumber.toLowerCase())) {
        throw new Error("That table already exists.");
    }
    const table = {
        id: nextId(demoAdminTables),
        tableNumber,
        qrToken: `demo-${Math.random().toString(36).slice(2, 10)}-${Date.now().toString(36)}`,
        isActive: true,
    };
    demoAdminTables.push(table);
    DemoStore.write("tables", demoAdminTables);
    return table;
}
function demoSetTableActive(id, isActive) {
    demoAdminTables = DemoStore.tables();
    const table = demoAdminTables.find((t) => t.id === id);
    if (!table) throw new Error("Table not found");
    table.isActive = isActive;
    DemoStore.write("tables", demoAdminTables);
    return table;
}

/* ---------------------------------------------------------------------- *
 * Dashboard / reports mirror
 * ---------------------------------------------------------------------- */
function demoBestSellersReport(count = 5) {
    const orders = DemoOrders.read().filter(({ status }) => status !== "Cancelled");
    return demoAdminProducts.map((product) => ({
        ...product,
        orderCount: orders.flatMap(({ items }) => items)
            .filter(({ itemNameSnapshot }) => itemNameSnapshot === product.name)
            .reduce((quantity, item) => quantity + item.quantity, 0),
    })).filter(({ orderCount }) => orderCount > 0)
        .sort((first, second) => second.orderCount - first.orderCount).slice(0, count);
}
function demoSalesSummary() {
    const today = new Date().toDateString();
    const orders = DemoOrders.read().filter((order) =>
        order.status !== "Cancelled" && new Date(order.createdAt).toDateString() === today);
    const revenueToday = orders.reduce((sum, order) => sum + order.total, 0);
    return {
        ordersToday: orders.length,
        revenueToday,
        averageOrderValue: orders.length ? revenueToday / orders.length : 0,
    };
}



/* ---------------------------------------------------------------------- *
 * Demo QR "code" - NOT a real, scannable QR. The real one is a PNG
 * generated server-side by the QRCoder library (see
 * 06-QR-CODE-AND-TABLE-FLOW.md section 4); this is only a visual
 * placeholder so the Tables page has something to show and download in
 * demo mode. It is clearly labeled as such everywhere it appears.
 * ---------------------------------------------------------------------- */
function demoQrCodeBlob(table) {
    const qr = new DemoQrEncoder(-1, 1);
    qr.addData(DemoStore.tableUrl(table.qrToken));
    qr.make();
    const size = qr.getModuleCount();
    const squares = [];
    for (let row = 0; row < size; row++) {
        for (let column = 0; column < size; column++) {
            if (qr.isDark(row, column)) squares.push(`M${column + 4},${row + 4}h1v1h-1z`);
        }
    }
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${size + 8} ${size + 8}">
        <rect width="100%" height="100%" fill="white"/>
        <path fill="black" d="${squares.join("")}"/>
    </svg>`;
    return new Blob([svg], { type: "image/svg+xml" });
}
