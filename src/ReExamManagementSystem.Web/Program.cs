using Microsoft.AspNetCore.Identity;
using ReExamManagementSystem.Application;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Infrastructure;
using ReExamManagementSystem.Infrastructure.Data.Seed;
using ReExamManagementSystem.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

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
