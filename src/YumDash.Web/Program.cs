using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;
using YumDash.Web.Data;
using YumDash.Web.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = NormalizePostgresConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection"));

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is required. Set it in configuration or via ConnectionStrings__DefaultConnection.");
}

builder.Services.AddControllersWithViews();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    foreach (var proxy in builder.Configuration.GetSection("ReverseProxy:KnownProxies").GetChildren())
    {
        var address = IPAddress.Parse(proxy.Value!);
        options.KnownProxies.Add(address);
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            options.KnownProxies.Add(address.MapToIPv6());
        }
    }
});
builder.Services
    .AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { ok = true }))
    .AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

app.MapControllerRoute(
    name: "admin-menu",
    pattern: "Admin/Menu/{action=Index}/{id?}",
    defaults: new { area = "Admin", controller = "MenuItems" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string NormalizePostgresConnectionString(string? rawConnectionString)
{
    if (string.IsNullOrWhiteSpace(rawConnectionString))
    {
        return string.Empty;
    }

    var normalized = rawConnectionString.Trim().Trim('\uFEFF').Trim().Trim('"', '\'');

    const string connectionStringPrefix = "ConnectionStrings__DefaultConnection=";
    if (normalized.StartsWith(connectionStringPrefix, StringComparison.OrdinalIgnoreCase))
    {
        normalized = normalized[connectionStringPrefix.Length..].Trim();
    }

    if (normalized.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        normalized.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return ConvertPostgresUriToConnectionString(normalized);
    }

    try
    {
        _ = new NpgsqlConnectionStringBuilder(normalized);
        return normalized;
    }
    catch (ArgumentException ex)
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is present but invalid. In Azure, set ConnectionStrings__DefaultConnection to only the PostgreSQL connection string value, without quotes or the setting name.",
            ex);
    }
}

static string ConvertPostgresUriToConnectionString(string connectionString)
{
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not a valid PostgreSQL URI.");
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
