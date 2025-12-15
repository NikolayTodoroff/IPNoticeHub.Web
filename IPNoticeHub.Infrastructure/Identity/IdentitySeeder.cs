using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using static IPNoticeHub.Shared.Support.RoleNames;

namespace IPNoticeHub.Infrastructure.Identity
{
    public class IdentitySeeder
    {
        public static async Task SeedIdentitiesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = 
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var logger = 
                scope.ServiceProvider.GetRequiredService<ILogger<IdentitySeeder>>();

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

            var adminEmail = AdminEmailAddress;
            var adminPassword = AdminEmailPassword;

            var adminUser = 
                await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var adminCreated = await userManager.CreateAsync(
                    adminUser, 
                    adminPassword);

                if (!adminCreated.Succeeded)
                {
                    var errors = adminCreated.Errors.Select(
                        e => e.Description);

                    logger.LogError(
                        "Failed to create admin user. Errors: {Errors}", 
                        string.Join("; ", errors));

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
                    var addPwdResult = 
                        await userManager.AddPasswordAsync(
                            adminUser,
                            adminPassword);

                    if (!addPwdResult.Succeeded)
                    {
                        var errors = addPwdResult.Errors.Select(
                            e => e.Description);

                        logger.LogWarning(
                            "Failed to add password to existing admin. Errors: {Errors}", 
                            string.Join("; ", errors));
                    }
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Admin);
            }

            if (!await userManager.IsInRoleAsync(adminUser, User))
            {
                await userManager.AddToRoleAsync(adminUser, User);
            }

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
