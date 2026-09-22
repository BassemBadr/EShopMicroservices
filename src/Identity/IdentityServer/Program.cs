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
// Configuration store: clients, API scopes, identity resources -> SQL Server
.AddConfigurationStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));
})
// Operational store: refresh tokens, persisted grants, sessions -> SQL Server
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName));

    // Cleanup expired grants/refresh tokens every hour (like a janitor)
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600;
})
.AddAspNetIdentity<ApplicationUser>()
.AddDeveloperSigningCredential();   //TODO: DEV ONLY — replace with certificate in production

// ---- 4. UI (login / consent pages)
builder.Services.AddRazorPages();

var app = builder.Build();
//  confgure the HTTP request pipeline

// ---- 5. Database: apply migrations automatically on startup (dev convenience)
await IdentityServerDbInitializer.InitializeAsync(app.Services);

app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();   // adds the /connect/* and /.well-known/* endpoints
app.UseAuthorization();
app.MapRazorPages();

app.Run();
