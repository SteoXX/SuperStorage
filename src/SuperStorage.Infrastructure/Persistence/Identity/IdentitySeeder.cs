using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Infrastructure.Persistence.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(
        this IServiceProvider serviceProvider,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in AuthRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new ApplicationRole
            {
                Name = roleName,
                Description = $"{roleName} role"
            };

            var roleResult = await roleManager.CreateAsync(role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create role '{roleName}': {FormatErrors(roleResult.Errors)}");
            }
        }

        var adminEmail = configuration["IdentitySeed:AdminEmail"];
        var adminPassword = configuration["IdentitySeed:AdminPassword"];
        var adminDisplayName = configuration["IdentitySeed:AdminDisplayName"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        var adminUser = existingUser ?? new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(adminDisplayName)
                ? "Administrator"
                : adminDisplayName.Trim()
        };

        if (existingUser is null)
        {
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create seed admin user: {FormatErrors(createResult.Errors)}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AuthRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, AuthRoles.Administrator);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to add seed admin user to Administrator role: {FormatErrors(roleResult.Errors)}");
            }
        }
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(error => error.Description));
    }
}
