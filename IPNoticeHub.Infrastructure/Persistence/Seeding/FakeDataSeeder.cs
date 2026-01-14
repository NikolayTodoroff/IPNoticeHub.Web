using Bogus;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkClasses;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkEntities;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateCopyrightEntities;
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

            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().
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

            var allTrademarks = await SeedTrademarkDataAsync(dbContext, logger);

            if (allTrademarks.Count == 0) return;

            await SeedDemoUserTrademarksAsync(dbContext, logger, demoUser, allTrademarks);
            await SeedDemoUserWatchlistAsync(dbContext, logger, demoUser, allTrademarks);
            await SeedDemoUserCopyrightsAsync(dbContext, logger, demoUser);
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
            var faker = new Faker("en");

            var demoUserTrademarkExists =
                await dbContext.UserTrademarks.
                AsNoTracking().
                AnyAsync(x => x.ApplicationUserId == demoUser.Id && !x.IsDeleted);

            if (!demoUserTrademarkExists)
            {
                var picks = PickDistinct(allTrademarks, count: 6);
                var now = DateTime.UtcNow;

                var links = 
                    picks.Select(entity => new UserTrademark
                {
                    ApplicationUserId = demoUser.Id,
                    TrademarkEntityId = entity.Id,
                    DateAdded = now.AddDays(-faker.Random.Number(1, 44)),
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

        private static async Task SeedDemoUserWatchlistAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            ApplicationUser demoUser,
            List<TrademarkEntity> allTrademarks)
        {
            var faker = new Faker("en");

            var demoWatchlistExists = 
                await dbContext.Watchlists.
                AsNoTracking().
                AnyAsync(w => w.UserId == demoUser.Id && !w.IsDeleted);

            if (!demoWatchlistExists)
            {
                var picks = PickDistinct(allTrademarks, count: 3);
                var now = DateTime.UtcNow;

                var watchList = 
                    picks.Select(entity => new Watchlist
                {
                    UserId = demoUser.Id,
                    TrademarkId = entity.Id,
                    IsDeleted = false,
                    NotificationsEnabled = faker.Random.Bool(),
                    AddedOnUtc = now.AddDays(-faker.Random.Number(1, 30)),
                    InitialStatusCodeRaw = entity.StatusCodeRaw,
                    InitialStatusText = entity.StatusDetail,
                    InitialStatusDateUtc = entity.StatusDateUtc
                });

                await dbContext.Watchlists.AddRangeAsync(watchList);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("FakeDataSeeder: " +
                    "Seeded demo user's watchlist (3 items).");
            }

            else
            {
                logger.LogInformation("FakeDataSeeder: " +
                    "Demo user's watchlist already exists. Skipping.");
            }
        }

        private static async Task SeedDemoUserCopyrightsAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            ApplicationUser demoUser)
        {
            var faker = new Faker("en");

            var demoCopyrightLinksExist = 
                await dbContext.UserCopyrights.
                AsNoTracking().
                AnyAsync(uc => uc.ApplicationUserId == demoUser.Id && !uc.IsDeleted);

            if (!demoCopyrightLinksExist)
            {
                var copyrights = GenerateCopyrights(count: 10);
                await dbContext.CopyrightRegistrations.AddRangeAsync(copyrights);
                await dbContext.SaveChangesAsync();

                var now = DateTime.UtcNow;

                var userLinks = 
                    copyrights.Select(entity => new UserCopyright
                {
                    ApplicationUserId = demoUser.Id,
                    CopyrightEntityId = entity.Id,
                    DateAdded = now.AddDays(-faker.Random.Number(1, 120)),
                    IsDeleted = false
                });

                await dbContext.UserCopyrights.AddRangeAsync(userLinks);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("FakeDataSeeder: " +
                    "Seeded demo user's copyrights (10 items).");
            }

            else
            {
                logger.LogInformation("FakeDataSeeder: " +
                    "Demo user's copyrights already exist. Skipping.");
            }

            logger.LogInformation("FakeDataSeeder: Done.");
        }
    }
}
