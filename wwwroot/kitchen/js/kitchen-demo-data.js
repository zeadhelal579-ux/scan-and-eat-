// Supplies sample tickets when the kitchen is running in demo mode.
function popNextDemoOrder() {
    const table = DemoStore.tables().find(({ isActive }) => isActive);
    if (!table) return null;
    return DemoOrders.place(table.qrToken, [
        { itemNameSnapshot: "Classic Cheeseburger", quantity: 1, unitPrice: 190, notes: "No onions" },
    ]);
}

function advanceDemoOrderStatus(orderId, newStatus) {
    return DemoOrders.advance(orderId, newStatus);
}

function demoListOpenOrders() {
    return DemoOrders.read().filter((order) => !["Delivered", "Cancelled"].includes(order.status));
}
