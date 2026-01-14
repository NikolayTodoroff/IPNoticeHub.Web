using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Shared.Enums;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;
using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class FakeDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var dbContext = 
                scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var logger = 
                scope.ServiceProvider.GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeDataSeeder");

            await dbContext.Database.MigrateAsync();

            var demoUser = await userManager.FindByEmailAsync(DemoUserEmailAddress);

            if (demoUser == null)
            {
                logger.LogWarning("FakeDataSeeder: Demo user not found (email: {Email}). " +
                    "Skipping fake data.", DemoUserEmailAddress);

                return;
            }
        }
    }
}
