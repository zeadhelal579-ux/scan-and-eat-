// Loads table orders and renders price-only checks from their saved line items.
const currency = new Intl.NumberFormat("en-EG", { style: "currency", currency: "EGP" });
const list = document.getElementById("order-list");
const search = document.getElementById("table-search");
const receipt = document.getElementById("receipt");
const printButton = document.getElementById("print-check");
const connection = document.getElementById("connection");
let orders = [];
let selectedId;
let loading = false;

function escapeHtml(value) {
    const span = document.createElement("span");
    span.textContent = value;
    return span.innerHTML;
}

function renderList() {
    const query = search.value.trim().toLowerCase();
    const visible = orders.filter((order) =>
        `table ${order.tableNumber} order ${order.id}`.toLowerCase().includes(query));
    list.innerHTML = visible.map((order) => `
        <button type="button" data-id="${order.id}" aria-pressed="${order.id === selectedId}">
            <span><strong>Table ${escapeHtml(order.tableNumber)}</strong><small>Order #${order.id}</small></span>
            <span>${currency.format(order.total)}</span>
        </button>`).join("") || "<p>No matching orders.</p>";
}

function renderReceipt() {
    const order = orders.find(({ id }) => id === selectedId);
    receipt.hidden = !order;
    printButton.hidden = !order;
    document.getElementById("check-empty").hidden = Boolean(order);
    if (!order) return;

    const rows = order.items.map((item) => `
        <tr><td>${escapeHtml(item.itemNameSnapshot)}</td><td>${item.quantity}</td>
        <td>${currency.format(item.unitPrice)}</td>
        <td>${currency.format(item.unitPrice * item.quantity)}</td></tr>`).join("");

    receipt.innerHTML = `
        <header><h2>Scan &amp; Eat</h2>
            <p>Table ${escapeHtml(order.tableNumber)} · Order #${order.id}</p>
            <p>${new Date(order.createdAt).toLocaleString()}${APP_CONFIG.DEMO_ONLY ? " · Demo check" : ""}</p>
        </header>
        <table>
            <thead><tr><th>Item</th><th>Qty</th><th>Unit</th><th>Amount</th></tr></thead>
            <tbody>${rows}</tbody>
        </table>
        <dl><dt>Subtotal</dt><dd>${currency.format(order.subtotal)}</dd>
            <dt>VAT</dt><dd>${currency.format(order.tax)}</dd>
            <dt><strong>Total</strong></dt><dd><strong>${currency.format(order.total)}</strong></dd>
        </dl>`;
}

async function refreshOrders() {
    if (loading) return;
    loading = true;
    try {
        const tickets = APP_CONFIG.DEMO_ONLY
            ? DemoOrders.read()
            : await StaffOrdersApi.listOpen("Pending,InPreparation,Ready,Delivered", "login.html");
        const nextOrders = tickets.filter(({ status }) => status !== "Cancelled").sort((a, b) => b.id - a.id);
        if (JSON.stringify(nextOrders) !== JSON.stringify(orders)) {
            const focusedId = document.activeElement?.closest("#order-list button")?.dataset.id;
            orders = nextOrders;
            renderList();
            renderReceipt();
            if (focusedId) list.querySelector(`[data-id="${Number(focusedId)}"]`)?.focus();
        }
        connection.textContent = APP_CONFIG.DEMO_ONLY ? "Demo · shared tickets" : "Orders up to date";
    } catch (error) {
        connection.textContent = "Couldn’t load orders. Retrying…";
    } finally {
        loading = false;
    }
}

if (APP_CONFIG.DEMO_ONLY || Auth.guardPage({ requireRole: "Staff", loginPageUrl: "login.html" })) {
    search.addEventListener("input", renderList);
    list.addEventListener("click", (event) => {
        const button = event.target.closest("[data-id]");
        if (!button) return;
        selectedId = Number(button.dataset.id);
        list.querySelectorAll("button").forEach((row) => row.setAttribute("aria-pressed", String(row === button)));
        renderReceipt();
    });
    printButton.addEventListener("click", () => window.print());
    renderList();
    refreshOrders();
    if (APP_CONFIG.DEMO_ONLY) DemoOrders.subscribe(refreshOrders);
    else setInterval(refreshOrders, 5000);
}
