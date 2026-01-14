using Bogus;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class GenerateTrademarkEntities
    {
        public static List<TrademarkEntity> GenerateTrademarks(int count)
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
                if (roll <= 50) return TrademarkStatusCategory.Registered;
                if (roll <= 70) return TrademarkStatusCategory.Pending;
                if (roll <= 90) return TrademarkStatusCategory.Abandoned;

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

                    var tempWordmark = faker.Random.Bool(0.6f) ?
                        baseName : $"{baseName} {faker.Random.Word().ToUpperInvariant()}";

                    if (tempWordmark.Length > 200) tempWordmark = tempWordmark[..200];

                    if (used.Add(tempWordmark)) return tempWordmark;
                }

                return faker.Company.CompanyName();
            }

            var list = new List<TrademarkEntity>(count);

            for (int i = 0; i < count; i++)
            {
                var trademarkClass = faker.PickRandom<TrademarkClass>();
                var goodsAndServices = trademarkClass.GetGoodsOnly();

                var status = PickStatus(faker);

                var filingDate = faker.Date.Past(
                    15, DateTime.UtcNow.AddMonths(-2)).Date;

                DateTime? registrationDate = status == TrademarkStatusCategory.Registered ? 
                    filingDate.AddDays(faker.Random.Int(200, 900)) : null;

                var statusDate = faker.Date.Between(filingDate, DateTime.UtcNow);

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
                    FilingDate = filingDate,
                    RegistrationDate = registrationDate,
                    MarkImageUrl = faker.Random.Bool(0.2f) ? faker.Internet.Url() : null,
                    Source = dataProvider
                });
            }

            return list;
        }
    }
}
