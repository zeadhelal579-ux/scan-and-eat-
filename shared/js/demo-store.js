// Shares demo catalog changes, table links and orders across tabs on this origin.
const DemoStore = (() => {
    const prefix = "scan-eat-preview-v2:";
    const fallbackToken = "demo-table-03";
    const initialTables = ["01", "02", "03", "04", "05 (patio)"].map((tableNumber, index) => ({
        id: index + 1,
        tableNumber,
        qrToken: `demo-table-0${index + 1}`,
        isActive: index !== 4,
    }));

    function read(name, initial) {
        const value = localStorage.getItem(prefix + name);
        return value === null ? structuredClone(initial) : JSON.parse(value);
    }

    function write(name, value) {
        localStorage.setItem(prefix + name, JSON.stringify(value));
        window.dispatchEvent(new CustomEvent(prefix + name));
    }

    function subscribe(name, listener) {
        window.addEventListener(prefix + name, listener);
        window.addEventListener("storage", (event) => {
            if (event.key === prefix + name) listener();
        });
    }

    const root = new URL(document.currentScript.src);

    function tableUrl(token) {
        const appRoot = root.pathname.replace(/\/(?:customer|admin|kitchen|cashier|shared)\/.*$|\/index\.html$/, "/");
        const url = new URL(appRoot.replace(/\/?$/, "/") + "customer/index.html", root.origin);
        url.searchParams.set("table", token);
        return url.href;
    }

    return {
        read, write, subscribe, tableUrl, fallbackToken,
        tables: () => read("tables", initialTables),
    };
})();
