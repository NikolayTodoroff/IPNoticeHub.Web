using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests
{
    public class UserTrademarkServiceBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ApplicationUser user = null!;
        protected ITrademarkRepository tmRepository = null!;
        protected IUserTrademarkRepository userTmRepository = null!;
        protected TrademarkCollectionService service = null!;

        protected TrademarkEntity tmEntity1 = null!;
        protected TrademarkEntity tmEntity2 = null!;
        protected TrademarkEntity tmEntity3 = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            user = InMemoryDbContextFactory.CreateApplicationUser();
            testDbContext.Users.Add(user);

            var testTrademarks =
                TestTrademarkData.CreateTestTrademarks(
                    out tmEntity1, out tmEntity2, out tmEntity3);

            testDbContext.TrademarkRegistrations.AddRange(testTrademarks);
            testDbContext.SaveChangesAsync();

            tmRepository = new TrademarkRepository(testDbContext);
            userTmRepository = new UserTrademarkRepository(testDbContext);

            service = new TrademarkCollectionService(
                tmRepository,
                userTmRepository);
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
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "testSourceId1",
                statusDetail: "testStatusDetail1",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

                (tmEntity2, _) =
                    InMemoryDbContextFactory.CreateTrademark(
                    wordmark: "BBB",
                    owner: "Owner B",
                    goodsAndServices: "testGoodsAndSerices2",
                    sourceId: "testSourceId2",
                    statusDetail: "testStatusDetail2",
                    regNumber: "7654321",
                    status: TrademarkStatusCategory.Cancelled,
                    source: DataProvider.EUIPO,
                    classNumbers: new[] { 9, 35 });

                (tmEntity3, _) =
                    InMemoryDbContextFactory.CreateTrademark(
                    wordmark: "CCC",
                    owner: "Owner C",
                    goodsAndServices: "testGoodsAndSerices3",
                    sourceId: "testSourceId3",
                    statusDetail: "testStatusDetail3",
                    regNumber: "4433221",
                    status: TrademarkStatusCategory.Pending,
                    source: DataProvider.WIPO,
                    classNumbers: new[] { 15 });

                return new[] { tmEntity1, tmEntity2, tmEntity3 };
            }
        }
    }
}
