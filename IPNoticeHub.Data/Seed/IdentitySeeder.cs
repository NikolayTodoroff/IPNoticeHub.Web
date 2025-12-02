using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IPNoticeHub.Common.Infrastructure;

namespace IPNoticeHub.Data.Seed
{
    public class IdentitySeeder
    {
        public static async Task SeedIdentitiesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.
                GetRequiredService<RoleManager< IdentityRole >>();

            var userManager = scope.ServiceProvider.
                GetRequiredService<UserManager< IdentityUser>>();

            var roleNames = new[]
            {
                RoleNames.Admin,
                RoleNames.User
            };

            foreach (var roleName in roleNames)
            {
                bool roleExists = await roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
