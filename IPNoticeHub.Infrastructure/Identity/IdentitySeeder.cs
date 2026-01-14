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

            var adminUser = await userManager.FindByEmailAsync(AdminEmailAddress);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmailAddress,
                    Email = AdminEmailAddress,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    adminUser,
                    AdminEmailPassword);

                if (!createResult.Succeeded)
                {
                    logger.LogCritical(
                        "Failed to create admin user. Errors: {Errors}",
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));

                    return;
                }

                logger.LogInformation($"Default admin ensured (email: ${AdminEmailAddress}.");
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
                    var pwdResult = await userManager.AddPasswordAsync(
                        adminUser,
                        AdminEmailPassword);

                    if (!pwdResult.Succeeded)
                    {
                        logger.LogWarning(
                            "Failed to add password to existing admin. Errors: {Errors}",
                            string.Join("; ", pwdResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            //
            var demoUser = await userManager.FindByEmailAsync(DemoUserEmailAddress);

            if (demoUser == null)
            {
                demoUser = new ApplicationUser
                {
                    UserName = DemoUserEmailAddress,
                    Email = DemoUserEmailAddress,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    demoUser,
                    DemoUserEmailPassword);

                if (!createResult.Succeeded)
                {
                    logger.LogCritical(
                        "Failed to create demo user. Errors: {Errors}",
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));

                    return;
                }

                logger.LogInformation($"Demo user ensured (email: ${DemoUserEmailAddress}.");
            }

            else
            {
                if (!demoUser.EmailConfirmed)
                {
                    demoUser.EmailConfirmed = true;
                    await userManager.UpdateAsync(demoUser);
                }

                if (!await userManager.HasPasswordAsync(demoUser))
                {
                    var pwdResult = await userManager.AddPasswordAsync(
                        demoUser,
                        DemoUserEmailPassword);

                    if (!pwdResult.Succeeded)
                    {
                        logger.LogWarning(
                            "Failed to add password to demo user. Errors: {Errors}",
                            string.Join("; ", pwdResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            //
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
        }
    }
}

