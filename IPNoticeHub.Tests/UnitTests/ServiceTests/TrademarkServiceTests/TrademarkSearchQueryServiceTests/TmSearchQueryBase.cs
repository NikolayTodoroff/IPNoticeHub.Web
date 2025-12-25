using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class TmSearchQueryBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ITrademarkReadRepository repository = null!;
        protected TrademarkSearchQueryService service = null!;

        protected TrademarkEntity anubisTm = null!;
        protected TrademarkEntity horusTm = null!;
        protected TrademarkEntity osirisTm = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testTrademarks = 
                TestTrademarkData.CreateTestTrademarks(
                    out anubisTm, 
                    out horusTm, out 
                    osirisTm);

            testDbContext.TrademarkRegistrations.AddRange(testTrademarks);
            testDbContext.SaveChangesAsync();

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

        protected static class TestTrademarkData
        {
            public static TrademarkEntity[] CreateTestTrademarks(
                out TrademarkEntity anubisTm, 
                out TrademarkEntity horusTm, 
                out TrademarkEntity osirisTm)
            {
                (anubisTm, _) =
                    InMemoryDbContextFactory.CreateTrademark(
                        wordmark: "Anubis",
                        owner: "Underworld Inc.",
                        goodsAndServices: "testGoodsAndSerices",
                        sourceId: "testSourceId",
                        statusDetail: "testStatusDetail",
                        regNumber: "1234567",
                        status: Shared.Enums.TrademarkStatusCategory.Registered,
                        source: Shared.Enums.DataProvider.USPTO,
                        classNumbers: new[] { 25 });

                (horusTm, _) =
                    InMemoryDbContextFactory.CreateTrademark(
                        wordmark: "Horus",
                        owner: "Falcon LLC",
                        goodsAndServices: "testGoodsAndSerices",
                        sourceId: "testSourceId",
                        statusDetail: "testStatusDetail",
                        regNumber: "1234512",
                        status: Shared.Enums.TrademarkStatusCategory.Pending,
                        source: Shared.Enums.DataProvider.WIPO,
                        classNumbers: new[] { 25,35 });

                (osirisTm, _) =
                    InMemoryDbContextFactory.CreateTrademark(
                        wordmark: "Osiris",
                        owner: "Afterlife Inc.",
                        goodsAndServices: "testGoodsAndSerices",
                        sourceId: "testSourceId",
                        statusDetail: "testStatusDetail",
                        regNumber: "3355442",
                        status: Shared.Enums.TrademarkStatusCategory.Cancelled,
                        source: Shared.Enums.DataProvider.EUIPO,
                        classNumbers: new[] { 5 });

                return new[] { anubisTm, horusTm, osirisTm };
            }
        }
    }
}
