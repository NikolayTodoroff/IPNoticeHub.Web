using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Common.ValidationConstants.AdminAccountCredentials;
using static IPNoticeHub.Common.Infrastructure.RoleNames;
using IPNoticeHub.Data.Entities.ApplicationUser;

namespace IPNoticeHub.Data.Seed
{
    public class IdentitySeeder
    {
        public static async Task SeedIdentitiesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentitySeeder>>();

            // Ensure required identity roles exist in the application; create any that are missing.
            var roleNames = new[]
            {
                Admin,
                User
            };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Ensure the administrator account exists for the configured email; create it if missing.
            // Verify and normalize email/password state: confirm the email and add a password if one is not set.
            var adminEmail = AdminEmailAddress;
            var adminPassword = AdminEmailPassword;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var adminCreated = await userManager.CreateAsync(adminUser, adminPassword);
                if (!adminCreated.Succeeded)
                {
                    var errors = adminCreated.Errors.Select(e => e.Description);
                    logger.LogError("Failed to create admin user. Errors: {Errors}", string.Join("; ", errors));
                    return;
                }
            }

            else
            {
                if (!adminUser.EmailConfirmed)
                {
                    adminUser.EmailConfirmed = true;
                    await userManager.UpdateAsync(adminUser);
                }

                if (!await userManager.HasPasswordAsync(adminUser))
                {
                    var addPwdResult = await userManager.AddPasswordAsync(adminUser, adminPassword);
                    if (!addPwdResult.Succeeded)
                    {
                        var errors = addPwdResult.Errors.Select(e => e.Description);
                        logger.LogWarning("Failed to add password to existing admin. Errors: {Errors}", string.Join("; ", errors));
                    }
                }
            }

            // Assign both Admin and User roles to the configured administrator if they are not already present.
            // This ensures the administrator has both elevated (Admin) and standard (User) privileges.
            if (!await userManager.IsInRoleAsync(adminUser, Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Admin);
            }

            if (!await userManager.IsInRoleAsync(adminUser, User))
            {
                await userManager.AddToRoleAsync(adminUser, User);
            }

            // Ensure every non-admin account has the User role to maintain consistent baseline permissions.
            var allUsers = await userManager.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                if (await userManager.IsInRoleAsync(user, Admin))
                {
                    continue;
                }

                if (!await userManager.IsInRoleAsync(user, User))
                {
                    await userManager.AddToRoleAsync(user, User);
                }
            }
        }
    }
}
