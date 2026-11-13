// FILE: Program.cs
// PURPOSE: The composition root (04-BACKEND-ARCHITECTURE.md section 6) -
//          the one place everything gets wired together before the app
//          starts handling requests.
// LOGIC:   - Registers AppDbContext, ASP.NET Core Identity + JWT auth,
//            SignalR (+ OrderHub mapped to /hubs/orders), CORS, every
//            Service, Swagger (with a Bearer auth button so protected
//            routes can be tried directly from the browser), and the
//            custom exception-handling middleware.
//          - The middleware order follows 04-BACKEND-ARCHITECTURE.md
//            section 6 exactly: "UseCors -> UseAuthentication ->
//            UseAuthorization -> MapControllers() / MapHub<OrderHub>()".
//          - JWT auth is configured to also accept the token from an
//            "access_token" query string parameter for requests under
//            /hubs - browsers cannot attach a custom Authorization header
//            to the WebSocket handshake SignalR's JS client uses, so this
//            is required for the Kitchen Display's live connection to
//            authenticate at all (see OrderHub.cs header comment).
//          - On startup: applies any pending EF Core migrations
//            (Database.Migrate()) and then runs the idempotent seeder
//            (Data/SeedData.cs). Database.Migrate() only APPLIES
//            migrations that already exist - the very first one still has
//            to be generated once, on a machine with the .NET SDK and a
//            reachable SQL Server, via:
//                dotnet ef migrations add InitialCreate
//            after which `dotnet run` (or dotnet ef database update)
//            creates the actual database and every run after that applies
//            anything new automatically.
//          - Serves the frontend (wwwroot/) as static files, so the
//            customer/kitchen/admin apps and this API run from the exact
//            same URL/port - "Option A - Single deployment" from
//            02-SYSTEM-ARCHITECTURE.md section 4, with zero CORS issues.
//            CORS is still enabled (AllowFrontendDev) so the frontend can
//            also be run separately (e.g. `python3 -m http.server`) during
//            local development, per that same document's note that "CORS
//            needs to be enabled for local development" either way.
//          - SECURITY HARDENING PASS (security-test-report.md): every
//            change below tagged with an "F<n>" comment traces back to that
//            report's numbered fix. Net effect: `dotnet run` locally still
//            works with zero extra setup (Development keeps its previous
//            conveniences - auto-migrate, a generated JWT key, seeded demo
//            accounts, permissive CORS), while every other environment now
//            defaults to the safer behavior in each case instead of having
//            to remember to change it later.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Hubs;
using ScanEat.Api.Models.Entities;
using ScanEat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Kestrel hardening (security-test-report.md F2 / F8) ----
// AddServerHeader=false stops the "Server: Kestrel" response header from
// advertising the stack to anyone probing the API. MaxRequestBodySize caps
// any single request at 100 KB - this app's largest legitimate payload is
// an order with a handful of line items (OrderDtos.cs already caps item
// count and note length server-side too), so 100 KB is generous headroom,
// not a tight fit, while still ruling out someone posting a multi-MB body
// at a public endpoint like POST /api/orders.
builder.WebHost.ConfigureKestrel(o =>
{
    o.AddServerHeader = false;
    o.Limits.MaxRequestBodySize = 100_000;
});

// ---- Controllers + consistent { message: ... } shape on validation errors ----
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Makes a Data-Annotations validation failure return the same
    // { "message": "..." } shape as a thrown ApiException
    // (see Helpers/ExceptionHandlingMiddleware.cs) instead of the default
    // ValidationProblemDetails shape - the frontend's own error handling
    // (shared/js/api-client.js) reads `parsedBody.message`.
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Invalid request.";
        return new BadRequestObjectResult(new { message = firstError });
    };
});

// ---- Swagger / OpenAPI (already present in the original template project) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Scan & Eat API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste just the token (no 'Bearer ' prefix - Swagger adds it). Get one from POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// ---- Database (SQL Server via EF Core Code-First) ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- ASP.NET Core Identity: staff accounts (Admin / Staff roles) ----
// SECURITY HARDENING PASS (see security-test-report.md F7 / F13):
// - RequiredLength raised 6 -> 12 and RequireUppercase turned on. There is
//   still no self-service signup in this app (AuthController only exposes
//   Login), so this mainly protects whoever sets the seeded/admin-created
//   account passwords - it's Identity's own built-in option, no extra code.
// - Lockout is new: AuthController now calls
//   SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)
//   instead of UserManager.CheckPasswordAsync, so these settings actually
//   take effect (they were previously configured nowhere and did nothing).
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 12;
        options.User.RequireUniqueEmail = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---- JWT Bearer authentication ----
// SECURITY HARDENING PASS (see security-test-report.md F1): appsettings.json
// no longer ships a real signing key (it previously committed one directly
// to source control - treat that old value as burned if it was ever
// deployed anywhere). The real key now has to come from outside the repo:
//   dotnet user-secrets set "Jwt:Key" "<openssl rand -base64 48>"   (local)
//   Jwt__Key environment variable                                   (server)
// In Development ONLY, a missing key is tolerated by generating a random
// one for this process (logged as a warning) so `dotnet run` still works
// immediately after a fresh clone with zero setup - it's different every
// restart, which is fine for local testing but means tokens don't survive
// a dev-server restart. Production refuses to start without a real key.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        builder.Configuration["Jwt:Key"] = jwtKey;
        Console.WriteLine(
            "[WARN] Jwt:Key missing/too short - using a random DEVELOPMENT-ONLY key for this run. " +
            "Set a real one via `dotnet user-secrets set \"Jwt:Key\" \"...\"` before this matters " +
            "(e.g. once you want tokens to survive a restart).");
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:Key is not configured (or is shorter than 32 chars). Set it via the Jwt__Key " +
            "environment variable or a secret store before starting in a non-Development environment.");
    }
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2),
        };

        // See file header comment - lets SignalR's WebSocket connection
        // authenticate via "?access_token=" instead of a header.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// SECURITY HARDENING PASS (see security-test-report.md F3): every one of
// the 26 existing endpoints already carries an explicit [Authorize] or
// [AllowAnonymous] attribute, so this FallbackPolicy changes nothing about
// them today - it's a deny-by-default safety net so a FUTURE controller
// action added without thinking about auth is rejected instead of silently
// public. Static files (wwwroot/) go through UseStaticFiles, not endpoint
// routing, so this does not touch the customer/kitchen/admin pages.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---- Rate limiting (security-test-report.md F7) ----
// Two named, per-client-IP policies: a tight one for the login endpoint
// (brute-force is the realistic threat there) and a looser one for order
// submission (the only other public write endpoint - guards against a
// script hammering the kitchen with fake orders). Both are on top of, not
// instead of, the Identity account lockout configured above - lockout
// protects one username, this protects the endpoint regardless of which
// username is being tried.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static string ClientKey(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    options.AddPolicy("login", ctx => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        ClientKey(ctx),
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
        }));

    options.AddPolicy("orders", ctx => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        ClientKey(ctx),
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
        }));
});

// ---- SignalR ----
builder.Services.AddSignalR();

// ---- CORS (security-test-report.md F9) ----
// AllowAnyOrigin is now Development-only convenience (so a frontend running
// from a different port during local dev, e.g. `python3 -m http.server`,
// still works with zero config). In every other environment this reads an
// explicit allow-list from config instead - "Cors:AllowedOrigins": [...]
// in appsettings.json (empty by default). Under "Option A - single
// deployment" (02-SYSTEM-ARCHITECTURE.md section 4, the recommended path -
// frontend served from this same app's wwwroot/) that list can just stay
// empty, since same-origin requests never go through CORS at all; only
// fill it in for "Option B - split deployment", with the real deployed
// frontend origin(s), e.g. ["https://menu.your-restaurant.com"].
// AllowCredentials() is never used here (this API is header-based JWT only,
// no cookies), so AllowAnyOrigin() in dev never combines with credentials.
const string CorsPolicyName = "AllowFrontendDev";
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else if (configuredOrigins.Length > 0)
        {
            policy.WithOrigins(configuredOrigins).AllowAnyMethod().AllowAnyHeader();
        }
        // else: no cross-origin policy at all outside Development - correct
        // default for Option A, where the frontend is always same-origin.
    });
});

// ---- Services (business logic - see Services/ folder) ----
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IRestaurantInfoService, RestaurantInfoService>();
builder.Services.AddScoped<JwtTokenGenerator>();

var app = builder.Build();

// ---- Apply pending migrations + idempotent seed (see file header comment) ----
// SECURITY HARDENING PASS (security-test-report.md F4): auto-applying
// migrations on every single process start (the previous behavior, in
// every environment) means the app's own DB login needs schema-changing
// (DDL) rights forever, not just at deploy time - broader permissions than
// the app needs day to day. Auto-migrate now only runs in Development, for
// the same "clone and dotnet run, zero setup" convenience as the Jwt:Key
// fallback above. Elsewhere, apply migrations as an explicit deploy step
// instead (`dotnet ef database update`, ideally with a separate, more
// privileged deploy-time credential than the app's normal runtime one),
// then this block just seeds - which only needs ordinary read/write rights.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        if (app.Environment.IsDevelopment())
        {
            var db = services.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        await SeedData.SeedAsync(services, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Startup database setup failed. Verify the connection and " +
            "apply the checked-in migrations with dotnet ef database update.");
        throw;
    }
}

// ---- Middleware pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // security-test-report.md F2 (A2): only meaningful outside Development,
    // where the app is actually expected to be reached over HTTPS long-term.
    app.UseHsts();
}

app.UseHttpsRedirection();

// ---- Security response headers (security-test-report.md F2 / A11) ----
// Runs for every response, including the static frontend files below.
// script-src has no 'unsafe-inline': the two inline <script> blocks that
// used to live directly in kitchen/login.html and admin/login.html were
// moved to kitchen/js/login-init.js and admin/js/login-init.js so this can
// stay strict. style-src keeps 'unsafe-inline' because several pages set
// style="..." directly (both in markup and from JS, e.g. the QR image's
// size) - CSS injection alone cannot run script, so this is a deliberate,
// much lower-risk trade-off, not an oversight.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' blob: https://images.unsplash.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "script-src 'self' https://cdnjs.cloudflare.com; " +
        "connect-src 'self'; " +
        "object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    await next();
});

// ---- Audit logging (security-test-report.md F14) ----
// The only logging that previously existed was for unhandled exceptions.
// This adds a lightweight trail for the events worth being able to look
// back at: blocked/unauthorized access attempts, and every admin write
// action (so "who changed this product" has an answer). Deliberately not
// logging full request bodies - just method, path, outcome, who, and from
// where - so this can't itself become a place secrets end up.
app.Use(async (context, next) =>
{
    await next();

    var path = context.Request.Path.Value ?? "";
    var status = context.Response.StatusCode;
    var method = context.Request.Method;
    var isAdminWrite = path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase)
        && !HttpMethods.IsGet(method);

    if (status is 401 or 403 or 429 || isAdminWrite)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AuditLog");
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var user = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity!.Name
            : "anonymous";
        logger.LogInformation("{Method} {Path} -> {Status} (user={User}, ip={Ip})", method, path, status, user, ip);
    }
});

// Serves frontend/customer, frontend/kitchen, frontend/admin, frontend/shared
// (copied into wwwroot/) as static files - see file header comment.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter(); // security-test-report.md F7 - must come after UseRouting

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();
