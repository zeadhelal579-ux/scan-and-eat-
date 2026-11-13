# Scan & Eat

Scan & Eat is a QR-based ordering system for dine-in restaurants. Guests scan the code on their table, browse the menu, and send an order with item-specific instructions. The kitchen sees the table and preparation notes, while the cashier can print an itemized check.

| Workspace | What you can do |
| --- | --- |
| **Guest menu** | Search dishes, browse categories and offers, and add notes such as “no onions” in the cart. |
| **Kitchen** | See incoming tickets by table, read preparation notes, and move orders through cooking, ready, and delivered. |
| **Cashier** | Find a table order and print a check with quantities, unit prices, VAT, and the total. |
| **Manager** | Edit the menu and offers, mark items sold out, and generate or deactivate table QR codes. |

## Explore the repository

| Branch | What's inside |
| --- | --- |
| [`front-Deploy`](../../tree/front-Deploy) | A static interactive demo with 38 sample dishes and local food photography. Orders and edits are shared between tabs in the same browser. |
| [`front-end`](../../tree/front-end) | The standalone customer and staff interfaces, configured to connect to the real API. |
| [`back-end`](../../tree/back-end) | The ASP.NET Core API, SQL Server migrations, SignalR hub, and a bundled copy of the frontend. |

## How an order moves through the restaurant

The table QR opens the menu with a table token. The backend validates the table and item availability, calculates prices from the catalog, and saves the order in SQL Server. SignalR delivers the ticket to the kitchen. The cashier uses the prices saved with the order when rendering the check.

**Stack** · HTML, CSS, JavaScript, ASP.NET Core 10, Entity Framework Core, SQL Server, and SignalR.

The SQL-backed order flow was tested locally in Chrome and Edge, including QR validation, kitchen status changes, and the cashier's print layout.
