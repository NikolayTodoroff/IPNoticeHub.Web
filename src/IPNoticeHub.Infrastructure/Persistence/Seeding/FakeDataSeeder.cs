using Bogus;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateCopyrightEntities;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkClasses;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.GenerateTrademarkEntities;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.PickDistinctElement;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class FakeDataSeeder
    {
        private static readonly DateTime SeedDateUtc = 
            new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        public static async Task SeedAsync(IServiceProvider services)
        {
            var dbContext =
                services.GetRequiredService<IPNoticeHubDbContext>();

            var userManager =
                services.GetRequiredService<UserManager<ApplicationUser>>();

            var logger = services.
                GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeDataSeeder");

            var demoUser = 
                await userManager.FindByEmailAsync(DemoUserEmailAddress);

            if (demoUser == null)
            {
                logger.LogWarning(
                    "Demo user account with email '{Email}' was not found. " +
                    "Fake data seeding will be skipped.", DemoUserEmailAddress);

                return;
            }

            Randomizer.Seed = new Random(20260101);
            var faker = new Faker("en");

            var allTrademarks = 
                await SeedTrademarkDataAsync(dbContext, logger);

            if (allTrademarks.Count == 0)
            {
                logger.LogWarning(
                    "No trademark registrations were found in the database after seeding. " +
                    "Demo user data seeding will be skipped.");

                return;
            }

            await SeedDemoUserTrademarksAsync(dbContext, logger, faker, demoUser, allTrademarks);
            await SeedDemoUserWatchlistAsync(dbContext, logger, faker, demoUser, allTrademarks);
            await SeedDemoUserCopyrightsAsync(dbContext, logger, faker, demoUser);

            logger.LogInformation("Fake data seeding completed successfully.");
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

                logger.LogInformation(
                    "Seeded {TrademarkCount} trademark registrations.",
                    trademarks.Count);
            }

            var allTrademarks =
                await dbContext.TrademarkRegistrations.
                AsNoTracking().
                OrderBy(t => t.Id).
                ToListAsync();

            if (allTrademarks.Count == 0) return allTrademarks;

            if (!await dbContext.Set<TrademarkClassAssignment>().AsNoTracking().AnyAsync())
            {
                var classes = 
                    GenerateTrademarkClassAssignments(allTrademarks);

                await dbContext.Set<TrademarkClassAssignment>().AddRangeAsync(classes);
                await dbContext.SaveChangesAsync();

                logger.LogInformation(
                    "Seeded {ClassAssignmentCount} trademark class assignments.",
                    classes.Count);
            }

            return allTrademarks;
        }

        private static async Task SeedDemoUserTrademarksAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            Faker faker,
            ApplicationUser demoUser,
            List<TrademarkEntity> allTrademarks)
        {
            var exists =
                await dbContext.UserTrademarks.
                AsNoTracking().
                AnyAsync(x => x.ApplicationUserId == demoUser.Id);

            if (exists)
            {
                logger.LogInformation(
                    "Demo user's trademark collection already contains data. Seeding skipped.");

                return;
            }

            var picks = PickDistinct(allTrademarks, count: 6);

            var links = picks.Select((
                entity, index) => new UserTrademark
            {
                ApplicationUserId = demoUser.Id,
                TrademarkEntityId = entity.Id,
                DateAdded = SeedDateUtc.AddDays(-faker.Random.Number(1, 44) - index),
                IsDeleted = false
            });

            await dbContext.UserTrademarks.AddRangeAsync(links);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Added 6 trademark registrations to the demo user's collection.");
        }

        private static async Task SeedDemoUserWatchlistAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            Faker faker,
            ApplicationUser demoUser,
            List<TrademarkEntity> allTrademarks)
        {
            var exists =
                await dbContext.Watchlists.
                AsNoTracking().
                AnyAsync(w => w.UserId == demoUser.Id);

            if (exists)
            {
                logger.LogInformation(
                    "Demo user's watchlist already contains data. Seeding skipped.");

                return;
            }

            var picks = PickDistinct(allTrademarks, count: 3);

            var watchList = picks.Select((
                entity, index) => new Watchlist
            {
                UserId = demoUser.Id,
                TrademarkId = entity.Id,
                IsDeleted = false,
                NotificationsEnabled = faker.Random.Bool(),
                AddedOnUtc = SeedDateUtc.AddDays(-faker.Random.Number(1, 30) - index),

                InitialStatusCodeRaw = entity.StatusCodeRaw,
                InitialStatusText = entity.StatusDetail,
                InitialStatusDateUtc = entity.StatusDateUtc
            });

            await dbContext.Watchlists.AddRangeAsync(watchList);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Added 3 trademark registrations to the demo user's watchlist.");
        }

        private static async Task SeedDemoUserCopyrightsAsync(
            IPNoticeHubDbContext dbContext,
            ILogger logger,
            Faker faker,
            ApplicationUser demoUser)
        {
            var exists =
                await dbContext.UserCopyrights.
                AsNoTracking().
                AnyAsync(uc => uc.ApplicationUserId == demoUser.Id);

            if (exists)
            {
                logger.LogInformation(
                    "Demo user's copyright collection already contains data. Seeding skipped.");

                return;
            }

            var copyrights = GenerateCopyrights(count: 10);

            await dbContext.CopyrightRegistrations.AddRangeAsync(copyrights);
            await dbContext.SaveChangesAsync();

            var userLinks = copyrights.Select((
                entity, index) => new UserCopyright
            {
                ApplicationUserId = demoUser.Id,
                CopyrightEntityId = entity.Id,
                DateAdded = SeedDateUtc.AddDays(-faker.Random.Number(1, 120) - index),
                IsDeleted = false
            });

            await dbContext.UserCopyrights.AddRangeAsync(userLinks);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Added 10 copyright registrations to the demo user's collection.");
        }
    }
}
