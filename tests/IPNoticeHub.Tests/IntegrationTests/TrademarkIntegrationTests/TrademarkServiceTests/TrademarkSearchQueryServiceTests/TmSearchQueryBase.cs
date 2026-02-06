using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class TmSearchQueryBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ITrademarkReadRepository repository = null!;
        protected TrademarkSearchQueryService service = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            repository =
                new TrademarkReadRepository(testDbContext);

            service =
                new TrademarkSearchQueryService(repository);
        }

        [TearDown]
        public void TearDown()
        {
            testDbContext?.Dispose();
        }
    }
}
