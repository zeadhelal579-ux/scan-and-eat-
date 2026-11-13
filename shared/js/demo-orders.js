// Seeds, places and updates the shared demo tickets.
const seedOrders = [
    {
        id: 478,
        tableNumber: "04",
        status: "Pending",
        items: [
            { itemNameSnapshot: "Monster Double Smash", quantity: 1, unitPrice: 210, notes: "No pickles" },
            { itemNameSnapshot: "Classic Cheeseburger", quantity: 2, unitPrice: 190, notes: null },
        ],
        subtotal: 590,
        tax: 82.6,
        total: 672.6,
        createdAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
        updatedAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
    },
    {
        id: 479,
        tableNumber: "02",
        status: "InPreparation",
        items: [
            { itemNameSnapshot: "Chicken Ranch Pizza", quantity: 1, unitPrice: 180, notes: null },
            { itemNameSnapshot: "Golden Dragon Roll (4 Pcs)", quantity: 1, unitPrice: 160, notes: null },
        ],
        subtotal: 340,
        tax: 47.6,
        total: 387.6,
        createdAt: new Date(Date.now() - 9 * 60 * 1000).toISOString(),
        updatedAt: new Date(Date.now() - 4 * 60 * 1000).toISOString(),
    },
    {
        id: 480,
        tableNumber: "01",
        status: "Ready",
        items: [{ itemNameSnapshot: "Twin Burger Pack", quantity: 1, unitPrice: 495, notes: null }],
        subtotal: 495,
        tax: 69.3,
        total: 564.3,
        createdAt: new Date(Date.now() - 15 * 60 * 1000).toISOString(),
        updatedAt: new Date(Date.now() - 1 * 60 * 1000).toISOString(),
    },
];


const DemoOrders = (() => {
    const key = "orders";
    const read = () => DemoStore.read(key, seedOrders);
    const transitions = {
        Pending: ["InPreparation", "Cancelled"],
        InPreparation: ["Ready", "Cancelled"],
        Ready: ["Delivered"],
    };

    function place(tableToken, items) {
        const table = DemoStore.tables().find((candidate) => candidate.qrToken === tableToken);
        if (!table?.isActive) throw new Error("This table is unavailable. Ask staff for an active table QR.");
        if (!items.length || items.length > 50 || items.some(({ quantity }) =>
            !Number.isInteger(quantity) || quantity < 1 || quantity > 50)) {
            throw new Error("Order up to 50 lines with 1 to 50 items each.");
        }
        const subtotalInCents = items.reduce((sum, item) =>
            sum + Math.round(item.unitPrice * 100) * item.quantity, 0);
        const taxInCents = Math.round(subtotalInCents * 0.14);
        const orders = read();
        const now = new Date().toISOString();
        const order = {
            id: Math.max(480, ...orders.map(({ id }) => id)) + 1,
            tableNumber: table.tableNumber,
            status: "Pending",
            items,
            subtotal: subtotalInCents / 100,
            tax: taxInCents / 100,
            total: (subtotalInCents + taxInCents) / 100,
            createdAt: now,
            updatedAt: now,
        };
        DemoStore.write(key, [...orders, order]);
        return order;
    }

    function advance(id, status) {
        const orders = read();
        const order = orders.find((candidate) => candidate.id === id);
        if (!transitions[order?.status]?.includes(status)) {
            throw new Error("The order has changed. Refresh the board and try again.");
        }
        order.status = status;
        order.updatedAt = new Date().toISOString();
        DemoStore.write(key, orders);
        return order;
    }

    return { read, place, advance, subscribe: (listener) => DemoStore.subscribe(key, listener) };
})();
