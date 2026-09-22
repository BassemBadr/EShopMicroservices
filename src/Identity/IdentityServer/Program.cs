using IdentityServer.Config;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//  configure services

// ---- 1. Database (users & roles stored here)
var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---- 2. ASP.NET Core Identity (user management: login, roles, claims)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Sensible defaults for a learning project
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ---- 3. IdentityServer (protocol engine + connect to our user store)
builder.Services.AddIdentityServer(options =>
{
    options.EmitStaticAudienceClaim = true;

    // Useful events for debugging during learning
    options.Events.RaiseSuccessEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseErrorEvents = true;
})
.AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
.AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
.AddInMemoryClients(IdentityServerConfig.Clients)
.AddAspNetIdentity<ApplicationUser>()
.AddDeveloperSigningCredential();   //TODO: DEV ONLY — replace with certificate in production

// ---- 4. UI (login / consent pages)
builder.Services.AddRazorPages();

var app = builder.Build();
//  confgure the HTTP request pipeline

// ---- 5. Database: apply migrations automatically on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();   // creates IdentityDb if missing, applies migrations

    // ---- 6. Seed admin user + roles (dev convenience)
    await SeedData.EnsureSeedData(scope.ServiceProvider);
}

app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();   // adds the /connect/* and /.well-known/* endpoints
app.UseAuthorization();
app.MapRazorPages();

app.Run();
