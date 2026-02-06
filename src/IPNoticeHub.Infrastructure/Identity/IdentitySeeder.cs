using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;
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

            bool allRolesCreated = true;

            foreach (var roleName in new[] { Admin, User })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (!result.Succeeded)
                    {
                        allRolesCreated = false;

                        logger.LogCritical(
                            "Failed to create role {Role}. Errors: {Errors}",
                            roleName,
                            string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            if (!allRolesCreated) return;

            var adminUser = await EnsureUserExistsAsync(
                userManager,
                logger,
                AdminEmailAddress,
                AdminEmailPassword,
                "admin");

            if (adminUser == null) return;

            var demoUser = await EnsureUserExistsAsync(
                userManager,
                logger,
                DemoUserEmailAddress,
                DemoUserEmailPassword,
                "demo user");

            if (demoUser == null) return;

            if (!await userManager.IsInRoleAsync(adminUser, Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Admin);
                logger.LogInformation("Admin role assigned to default admin.");
            }

            if (!await userManager.IsInRoleAsync(adminUser, User))
            {
                await userManager.AddToRoleAsync(adminUser, User);
                logger.LogInformation("User role assigned to default admin.");
            }

            if (!await userManager.IsInRoleAsync(demoUser, User))
            {
                await userManager.AddToRoleAsync(demoUser, User);
                logger.LogInformation("User role assigned to demo user.");
            }
        }

        private static async Task<ApplicationUser?> EnsureUserExistsAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            string email,
            string password,
            string displayNameForLogs)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    logger.LogCritical(
                        "Failed to create {UserType}. Errors: {Errors}",
                        displayNameForLogs,
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));

                    return null;
                }

                logger.LogInformation("Default {UserType} ensured (email: {Email}).", displayNameForLogs, email);

            }
            else
            {
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.HasPasswordAsync(user))
                {
                    var pwdResult = await userManager.AddPasswordAsync(user, password);

                    if (!pwdResult.Succeeded)
                    {
                        logger.LogWarning(
                            "Failed to add password to existing {UserType}. Errors: {Errors}",
                            displayNameForLogs,
                            string.Join("; ", pwdResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            return user;
        }
    }
}

