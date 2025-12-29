using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests
{
    public class TmSearchServiceBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ITrademarkRepository repository = null!;
        protected TrademarkSearchService service = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            repository =
                new TrademarkRepository(testDbContext);

            service =
                new TrademarkSearchService(repository);
        }

        [TearDown]
        public void TearDown()
        {
            testDbContext?.Dispose();
        }
    }
}
