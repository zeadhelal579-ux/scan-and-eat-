// Sets the entry points for a backend-connected site or a self-contained demo.
if (APP_CONFIG.DEMO_ONLY) {
    const badge = document.getElementById("mode-badge");
    badge.textContent = "Interactive demo";
    badge.hidden = false;
    document.getElementById("demo-hint").hidden = false;
    function updateMenuLink() {
        const activeTables = DemoStore.tables().filter(({ isActive }) => isActive);
        const table = activeTables.find(({ qrToken }) => qrToken === DemoStore.fallbackToken) || activeTables[0];
        document.getElementById("menu-link").href = table ? DemoStore.tableUrl(table.qrToken) : "customer/index.html";
    }
    updateMenuLink();
    DemoStore.subscribe("tables", updateMenuLink);
    document.getElementById("kitchen-link").href = "kitchen/index.html";
    document.getElementById("admin-link").href = "admin/dashboard.html";
}
