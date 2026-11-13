# Scan & Eat — Backend

The ASP.NET Core 10 application handles the menu, table QR codes, staff accounts, orders, and reports. Entity Framework Core persists the data in SQL Server. SignalR sends new tickets and status changes to the kitchen. The server also serves the customer, kitchen, cashier, and manager pages from `wwwroot/`.

## Run locally

Install the .NET 10 SDK and SQL Server LocalDB on Windows. From the directory containing the project file, run:

```powershell
sqllocaldb start MSSQLLocalDB
dotnet restore
dotnet run --launch-profile https
```

Open `https://localhost:7052/` for the application or `https://localhost:7052/swagger` for the development API reference. Development applies the checked-in migrations and seeds a fresh database with 38 dishes, 8 categories, 5 offers, and 5 tables. Existing catalog data is preserved.

Development seeds an `admin` user and a `kitchen` staff user by default. The local seed password is defined in `Data/SeedData.cs`. Demo accounts are disabled by default outside Development.

## API and order workflow

| Surface | Responsibility |
| --- | --- |
| `GET /api/products`, `/api/categories`, `/api/offers` | Public menu data. |
| `GET /api/tables/current` | Resolve an active table token to a table label. |
| `POST /api/orders` | Validate the table and item availability, calculate prices and VAT, and save the order. |
| `/api/staff/orders` | Read tickets and update allowed statuses. Requires Staff or Admin. |
| `/api/admin/*` | Catalog and table management, QR images, and reports. Requires Admin. |
| `/hubs/orders` | Authenticated SignalR notifications for new orders and status updates. |

Orders retain the item names and unit prices used at checkout. Combo orders also save their component names and quantities for the kitchen. The cashier's receipt shows the financial lines and omits preparation notes.

## Deploy with SQL Server

| Configuration | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string. |
| `Jwt__Key` | JWT signing secret of at least 32 characters. |
| `FrontendBaseUrl` | Public frontend URL encoded in generated table QR codes. |
| `Cors__AllowedOrigins__0` | Allowed frontend origin when serving the UI separately. |
| `SeedDemoAccounts` | Keep disabled for deployed environments. |

Apply the checked-in migrations with `dotnet ef database update` before starting the production app. If an existing database was created from a separately generated baseline, reconcile its EF migration history before upgrading. Supply secrets through the hosting environment and provision staff accounts for that environment.

The frontend can run from `wwwroot/` on the same origin or separately from the [`front-end` branch](../../tree/front-end).
