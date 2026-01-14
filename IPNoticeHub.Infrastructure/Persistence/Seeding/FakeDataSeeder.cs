using Bogus;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkClasses;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkEntities;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.PickDistinctElement;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;

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

            Randomizer.Seed = new Random(20250101);

            var allTrademarks = 
                await SeedTrademarkDataAsync(dbContext, logger);

            if (allTrademarks.Count == 0) return;

            await SeedDemoUserTrademarksAsync(dbContext, logger, demoUser, allTrademarks);
        }

        private static async Task<List<TrademarkEntity>> SeedTrademarkDataAsync(
            IPNoticeHubDbContext dbContext, 
            ILogger logger)
        {
            if (!await dbContext.TrademarkRegistrations.AsNoTracking().AnyAsync())
            {
                var trademarks = GenerateTrademarks(count: 60);
                await dbContext.TrademarkRegistrations.AddRangeAsync(trademarks);
                await dbContext.SaveChangesAsync();

                var classes = GenerateTrademarkClassAssignments(trademarks);
                await dbContext.Set<TrademarkClassAssignment>().AddRangeAsync(classes);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("FakeDataSeeder: Seeded {Count} global trademarks and " +
                    "class assignments.", trademarks.Count);
            }
            else
            {
                logger.LogInformation("FakeDataSeeder: Global trademarks already exist. " +
                    "Skipping global trademark seeding.");
            }

            var allTrademarks = 
                await dbContext.TrademarkRegistrations.
                AsNoTracking().
                OrderBy(t => t.Id).
                ToListAsync();

            if (allTrademarks.Count == 0)
            {
                logger.LogWarning("FakeDataSeeder: No trademarks found after seeding. " +
                    "Skipping demo links.");
            }

            return allTrademarks;
        }

        private static async Task SeedDemoUserTrademarksAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            ApplicationUser demoUser,
            List<TrademarkEntity> allTrademarks)
        {
            var demoUserTrademarkExists =
                await dbContext.UserTrademarks.
                AsNoTracking().
                AnyAsync(x => x.ApplicationUserId == demoUser.Id && !x.IsDeleted);

            if (!demoUserTrademarkExists)
            {
                var picks = PickDistinct(allTrademarks, count: 6);
                var now = DateTime.UtcNow;

                var links = picks.Select(entity => new UserTrademark
                {
                    ApplicationUserId = demoUser.Id,
                    TrademarkEntityId = entity.Id,
                    DateAdded = now.AddDays(-Random.Shared.Next(1, 45)),
                    IsDeleted = false
                });

                await dbContext.UserTrademarks.AddRangeAsync(links);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("FakeDataSeeder: " +
                    "Seeded demo user's trademark collection (6 items).");
            }
            else
            {
                logger.LogInformation("FakeDataSeeder: " +
                    "Demo user's trademark collection already exists. Skipping.");
            }
        }
    }
}
