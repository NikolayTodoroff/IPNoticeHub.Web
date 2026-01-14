using Bogus;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Formats.Asn1;
using static IPNoticeHub.Infrastructure.Persistence.Seeding.TrademarkClassExtensions;
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

            if (!await dbContext.TrademarkRegistrations.AsNoTracking().AnyAsync())
            {
                var trademarks = GenerateTrademarks(count: 60);
                await dbContext.TrademarkRegistrations.AddRangeAsync(trademarks);
                await dbContext.SaveChangesAsync();

                var classes = GenerateTrademarkClassAssignments(trademarks);
                await dbContext.TrademarkClassAssignments.AddRangeAsync(classes);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("FakeDataSeeder: Seeded {Count} global trademarks and " +
                    "class assignments.", trademarks.Count);
            }
            else
            {
                logger.LogInformation("FakeDataSeeder: Global trademarks already exist. " +
                    "Skipping global trademark seeding.");
            }
        }

        private static List<TrademarkEntity> GenerateTrademarks(int count)
        {
            var faker = new Faker("en");

            var brands = new[]
            {
                "Nike","Adidas","Puma","Apple","Google","Microsoft","Amazon","Meta",
                "Netflix","Spotify","Tesla","Samsung","Sony","LG","Intel","AMD","NVIDIA",
                "Cisco","Oracle","IBM","Coca-Cola","Pepsi","Starbucks","McDonald's",
                "IKEA","Zara","H&M","Uniqlo","Shopify","Stripe","Cloudflare","Atlassian",
                "Slack","Zoom","Dropbox","Activision","VaultSentry","EA",
                "Lenovo","Samsung"
            };

            TrademarkStatusCategory PickStatus(Faker faker)
            {
                var roll = faker.Random.Int(1, 100);
                if (roll <= 40) return TrademarkStatusCategory.Registered;
                if (roll <= 60) return TrademarkStatusCategory.Pending;
                if (roll <= 80) return TrademarkStatusCategory.Abandoned;
                return TrademarkStatusCategory.Cancelled;
            }

            string StatusDetail(TrademarkStatusCategory s) => s switch
            {
                TrademarkStatusCategory.Registered => "Live/Registered",
                TrademarkStatusCategory.Pending => "Live/Pending",
                TrademarkStatusCategory.Abandoned => "Dead/Abandoned",
                TrademarkStatusCategory.Cancelled => "Dead/Cancelled",
                _ => "Other"
            };

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string NextWordmark()
            {
                for (var i = 0; i < 40; i++)
                {
                    var baseName = faker.PickRandom(brands);
                    var variant = faker.Random.Bool(0.6f)
                        ? baseName
                        : $"{baseName} {faker.Random.Word().ToUpperInvariant()}";

                    if (variant.Length > 200) variant = variant[..200];

                    if (used.Add(variant)) return variant;
                }

                return faker.Company.CompanyName();
            }

            var list = new List<TrademarkEntity>(count);

            for (int i = 0; i < count; i++)
            {
                var trademarkClass = faker.PickRandom<TrademarkClass>();
                var goodsAndServices = trademarkClass.GetGoodsOnly();

                var status = PickStatus(faker);

                var filing = faker.Date.Past(
                    15, DateTime.UtcNow.AddMonths(-2)).Date;

                DateTime? registration = status == TrademarkStatusCategory.Registered
                    ? filing.AddDays(faker.Random.Int(200, 900))
                    : null;

                var statusDate = faker.Date.Between(filing, DateTime.UtcNow);

                var sourceId = "USPTO-" + faker.Random.ReplaceNumbers("########");

                var dataProvider = faker.PickRandom<DataProvider>();

                list.Add(new TrademarkEntity
                {
                    PublicId = Guid.NewGuid(),
                    Wordmark = NextWordmark(),
                    SourceId = sourceId,
                    RegistrationNumber = status == TrademarkStatusCategory.Registered ? 
                    faker.Random.ReplaceNumbers("########") : null,
                    GoodsAndServices = goodsAndServices,
                    Owner = faker.Company.CompanyName(),
                    StatusCategory = status,
                    StatusDetail = StatusDetail(status),
                    StatusCodeRaw = faker.Random.Bool(0.7f) ? faker.Random.Int(100, 999) : null,
                    StatusDateUtc = statusDate,
                    FilingDate = filing,
                    RegistrationDate = registration,
                    MarkImageUrl = faker.Random.Bool(0.2f) ? faker.Internet.Url() : null,
                    Source = dataProvider
                });
            }

            return list;
        }

        private static List<TrademarkClassAssignment> GenerateTrademarkClassAssignments(List<TrademarkEntity> trademarks)
        {
            var result = new List<TrademarkClassAssignment>();

            int[] DeriveClasses(string goods)
            {
                goods = goods.ToLowerInvariant();
                if (goods.Contains("clothing") || goods.Contains("footwear")) return new[] { 25, 18 };
                if (goods.Contains("software") || goods.Contains("saas") || goods.Contains("applications")) return new[] { 9, 42 };
                if (goods.Contains("security") || goods.Contains("incident")) return new[] { 42, 45 };
                if (goods.Contains("retail") || goods.Contains("marketplace")) return new[] { 35 };
                if (goods.Contains("beverages") || goods.Contains("soft drinks")) return new[] { 32 };
                if (goods.Contains("furniture") || goods.Contains("home")) return new[] { 20, 11 };
                return new[] { 9 };
            }

            foreach (var t in trademarks)
            {
                var classes = DeriveClasses(t.GoodsAndServices);

                foreach (var c in classes.Take(2).Distinct())
                {
                    result.Add(new TrademarkClassAssignment
                    {
                        TrademarkRegistrationId = t.Id,
                        ClassNumber = c
                    });
                }
            }

            return result;
        }
    }
}
