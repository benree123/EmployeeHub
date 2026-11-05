using System.Security.Claims;
using System.Threading.RateLimiting;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var useSqlite = builder.Configuration["DatabaseProvider"] == "Sqlite";
var demoEnabled = builder.Configuration.GetValue<bool>("Demo:Enabled");
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);

builder.Services.AddControllersWithViews();

// SQL Server uses migrations. SQLite supports the hosted demo and fast tests.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlite)
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
            ?? $"Data Source={Path.Combine(dataDirectory, "employeehub.db")}");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddSingleton(Accounts.Load(builder.Configuration, dataDirectory));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "EmployeeHub.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = false;
});
builder.Services.AddAuthorization(options =>
{
    // Every page requires sign-in unless it opts out with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(Roles.CanEditPolicy, policy => policy.RequireRole(Roles.Admin, Roles.DemoGuest));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    FixedWindowRateLimiterOptions Limit(int permits) => new() { PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 };
    string Address(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var login = builder.Configuration.GetValue("RateLimits:Login", 5);
    var writes = builder.Configuration.GetValue("RateLimits:Writes", 30);
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(Address(context), _ => Limit(login)));
    options.AddPolicy("writes", context => RateLimitPartition.GetFixedWindowLimiter(Address(context), _ => Limit(writes)));
});

if (demoEnabled) builder.Services.AddHostedService<DemoResetService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (useSqlite) context.Database.EnsureCreated();
    else context.Database.Migrate();
    DbInitializer.Initialize(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
