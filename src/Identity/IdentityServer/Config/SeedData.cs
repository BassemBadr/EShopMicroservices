using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Config;

public static class SeedData
{
    public static async Task EnsureSeedData(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // ---- 1. Create roles
        string[] roles = { "Admin", "Customer", "Manager" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // ---- 2. Create the admin user
        const string adminEmail = "admin@eshop.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(admin, "P@ssw0rd1!");

            if (result.Succeeded)
            {
                // Assign the Admin role
                await userManager.AddToRoleAsync(admin, "Admin");

                // Give the admin a role claim so it shows up in tokens
                await userManager.AddClaimAsync(admin, new Claim("role", "Admin"));
            }
        }
    }
}
