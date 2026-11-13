# Scan & Eat — Standalone frontend

This branch contains the browser interfaces for guests, kitchen staff, cashiers, and managers. It uses plain HTML, CSS, and JavaScript and connects to the ASP.NET Core API and SignalR hub.

## Run locally

1. Start the server from the [`back-end` branch](../../tree/back-end) with its HTTPS launch profile at `https://localhost:7052`.
2. From this branch's root, serve the frontend with Python 3.

   ```powershell
   python -m http.server 8080
   ```

3. Open `http://localhost:8080/`. Sign in as a manager and open **Tables** to generate a QR link.

## Pages and source files

| Path | Purpose |
| --- | --- |
| `customer/` | Menu, search, offers, cart, table validation, and preparation notes. |
| `kitchen/` | Staff sign-in and ticket board with status filters, SignalR updates, and polling fallback. |
| `cashier/` | Order lookup and printable checks with prices saved by the backend. |
| `admin/` | Manager pages for catalog, offers, tables, and QR codes. |
| `shared/` | Staff authentication, API client, and shared styles. |
| `assets/menu/` | Bundled WebP food photos. |

## Connect to another backend

Set `BACKEND_URL` in both `customer/js/config.js` and `shared/js/config.js` to the API's HTTPS origin without the `/api` suffix. For separate hosting, set the backend's `Cors__AllowedOrigins__0` to the frontend origin and `FrontendBaseUrl` to the public frontend URL used in table QR codes.

The connected frontend reports API connection errors and requires a valid table link to place an order.
