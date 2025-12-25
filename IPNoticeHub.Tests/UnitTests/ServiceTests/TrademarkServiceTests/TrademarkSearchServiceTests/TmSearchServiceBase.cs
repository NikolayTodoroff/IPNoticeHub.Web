using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests
{
    public class TmSearchServiceBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ITrademarkRepository repository = null!;
        protected TrademarkSearchService service = null!;

        protected TrademarkEntity tmEntity1 = null!;
        protected TrademarkEntity tmEntity2 = null!;
        protected TrademarkEntity tmEntity3 = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testTrademarks =
                TestTrademarkData.CreateTestTrademarks(
                    out tmEntity1, 
                    out tmEntity2, 
                    out tmEntity3);

            testDbContext.TrademarkRegistrations.AddRange(testTrademarks);
            testDbContext.SaveChangesAsync();

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

        protected static class TestTrademarkData
        {
            public static TrademarkEntity[] CreateTestTrademarks(
                out TrademarkEntity tmEntity1,
                out TrademarkEntity tmEntity2,
                out TrademarkEntity tmEntity3)
            {
                (tmEntity1, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers:new[]{ 25, 35 });

                (tmEntity2, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Cancelled,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 15, 21 });

                (tmEntity3, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.WIPO);

                return new[] { tmEntity1, tmEntity2, tmEntity3 };
            }
        }
    }
}
