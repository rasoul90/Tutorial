using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Rasad.Infrastructure.Identity;

namespace Rasad.Web.Identity;

public static class IdentitySeed
{
    private static readonly string[] Roles =
    {
        "SuperAdmin",
        "MinistryAdmin",
        "MinistryUser"
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = configuration.GetSection(SuperAdminOptions.SectionName).Get<SuperAdminOptions>()
                      ?? new SuperAdminOptions();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        var existingUser = await userManager.FindByNameAsync(options.UserName);
        if (existingUser is null)
        {
            var user = new ApplicationUser
            {
                UserName = options.UserName,
                Email = options.Email
            };

            var result = await userManager.CreateAsync(user, options.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
    }
}
