using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServer.Config;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data;

public static class IdentityServerDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // 1. User store (existing)
        var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await identityDb.Database.MigrateAsync();

        // 2. Configuration store (clients/scopes/resources)
        var configDb = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        await configDb.Database.MigrateAsync();
        await SeedConfigurationAsync(configDb,env.IsDevelopment());

        // 3. Operational store (grants/refresh tokens)
        var persistedDb = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        await persistedDb.Database.MigrateAsync();

        // 4. Seed users/roles
        await SeedData.EnsureSeedData(services);
    }

    private static async Task SeedConfigurationAsync(ConfigurationDbContext context,bool resetInDev)
    {
        if (resetInDev)
        {
            // Development: the C# config classes are the source of truth.
            // Fall through to the empty checks below and insert fresh rows.
            context.Clients.RemoveRange(context.Clients);
            context.IdentityResources.RemoveRange(context.IdentityResources);
            context.ApiScopes.RemoveRange(context.ApiScopes);
            await context.SaveChangesAsync();
        }

        // Only seed if the tables are empty (idempotent — safe on every startup)
        if (!context.Clients.Any())
        {
            foreach (var client in IdentityServerConfig.Clients)
                context.Clients.Add(client.ToEntity());

            await context.SaveChangesAsync();
        }

        if (!context.IdentityResources.Any())
        {
            foreach (var resource in IdentityServerConfig.IdentityResources)
                context.IdentityResources.Add(resource.ToEntity());

            await context.SaveChangesAsync();
        }

        if (!context.ApiScopes.Any())
        {
            foreach (var apiScope in IdentityServerConfig.ApiScopes)
                context.ApiScopes.Add(apiScope.ToEntity());

            await context.SaveChangesAsync();
        }
    }
}
