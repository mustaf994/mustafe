using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using ReExamManagementSystem.Application;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Infrastructure;
using ReExamManagementSystem.Infrastructure.Data.Seed;
using ReExamManagementSystem.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Render (and most other PaaS hosts) hand the database connection as a single
// postgres:// URI in DATABASE_URL rather than the ConnectionStrings__DefaultConnection
// key/value format Npgsql expects, so translate it here before it's read below.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] = ConvertDatabaseUrlToNpgsqlConnectionString(databaseUrl);
}

// Render assigns the container a port at runtime via PORT and routes its own
// HTTPS edge to it over plain HTTP - Kestrel must bind to that exact port.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Layered composition: each layer exposes its own AddXServices() extension
// so Program.cs stays a thin wiring point instead of a dumping ground.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password policy — enforced server-side by Identity's hasher/validator,
        // never re-implemented ad hoc in a controller.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ReExamManagementSystem.Infrastructure.Data.ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministrator", policy => policy.RequireRole(Roles.Administrator));
    options.AddPolicy("RequireExaminationOfficer", policy => policy.RequireRole(Roles.ExaminationOfficer));
    options.AddPolicy("RequireStudent", policy => policy.RequireRole(Roles.Student));
    options.AddPolicy("RequireStaff", policy => policy.RequireRole(Roles.Administrator, Roles.ExaminationOfficer));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var startupScope = app.Services.CreateScope())
{
    var logger = startupScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await DbInitializer.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations or seeding the database.");
        // The stack trace above pushes the actual cause off-screen in hosted log
        // viewers, so repeat it as the last line before the process exits.
        Console.Error.WriteLine($"DATABASE STARTUP FAILED: {ex.GetBaseException().Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Friendly branded pages for 404/403/etc. in every environment - the raw
// framework default status pages are not something end users should see.
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

// Behind Render's (or any reverse proxy's) TLS-terminating edge, the request
// that reaches Kestrel is plain HTTP - without this, UseHttpsRedirection below
// would see it as insecure and redirect again, producing a loop.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// postgres://user:password@host:port/database -> Npgsql's Host=...;Port=...;... form.
// Render's managed Postgres requires SSL for connections from outside its private
// network, while the internal URL may not offer it - "Prefer" uses SSL when the
// server supports it. Its certificates aren't in the container's trust store, hence
// "Trust Server Certificate=true" rather than validating against a CA.
static string ConvertDatabaseUrlToNpgsqlConnectionString(string databaseUrl)
{
    var trimmed = databaseUrl.Trim();

    // Render's dashboard sometimes renders this value without the "//" after
    // the scheme (e.g. "postgres:user:pass@host/db"). System.Uri treats a
    // scheme with no "//" as opaque (like "mailto:") and parses it with an
    // empty host and no user info at all rather than failing - normalizing
    // it to a proper "scheme://" URI first is what actually fixed this.
    if (!trimmed.Contains("://", StringComparison.Ordinal))
    {
        var schemeSeparator = trimmed.IndexOf(':');
        if (schemeSeparator > 0)
        {
            trimmed = trimmed[..schemeSeparator] + "://" + trimmed[(schemeSeparator + 1)..];
        }
    }

    Uri uri;
    try
    {
        uri = new Uri(trimmed);
    }
    catch (UriFormatException ex)
    {
        throw new InvalidOperationException(
            $"DATABASE_URL isn't a valid URI (expected postgres://user:password@host:port/database). " +
            $"Got {trimmed.Length} character(s), starting with '{Left(trimmed, 12)}'.", ex);
    }

    // An opaque/malformed URI (e.g. missing the "//" after the scheme) parses
    // without throwing but leaves UserInfo empty, which is where this used to
    // crash with an unhelpful IndexOutOfRangeException instead of this message.
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2 || userInfo[0].Length == 0)
    {
        throw new InvalidOperationException(
            $"DATABASE_URL is missing 'user:password@' credentials (expected postgres://user:password@host:port/database). " +
            $"Parsed scheme='{uri.Scheme}', host='{uri.Host}'.");
    }

    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = Uri.UnescapeDataString(userInfo[1]);
    var host = ResolveRenderDatabaseHost(uri.Host);

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};" +
        "SSL Mode=Prefer;Trust Server Certificate=true";
}

// Render's internal database hostname (e.g. "dpg-xxxx-a", no domain) only resolves
// for services in the same region as the database. When it doesn't resolve, fall
// back to the external hostname - Render's Postgres edge routes by the database id,
// so any regional external domain reaches it (over SSL, which "Prefer" negotiates).
static string ResolveRenderDatabaseHost(string host)
{
    if (!host.StartsWith("dpg-", StringComparison.Ordinal) || host.Contains('.'))
    {
        return host;
    }

    try
    {
        System.Net.Dns.GetHostAddresses(host);
        return host;
    }
    catch (System.Net.Sockets.SocketException)
    {
        var region = Environment.GetEnvironmentVariable("RENDER_DATABASE_REGION") ?? "oregon";
        return $"{host}.{region}-postgres.render.com";
    }
}

static string Left(string value, int count) => value.Length <= count ? value : value[..count];
