using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoInputSanityTests
    {
        [Test]
        public void QueryRepository_WithDefaultFilter_ReturnsAllTrademarks()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA1",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA2",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices0",
                sourceId: "testSourceId0",
                statusDetail: "testStatusDetail0",
                regNumber: "7654321",
                TrademarkStatusCategory.Pending,
                DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                firstTestTrademark, 
                secondTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(
                new TrademarkSearchFilter()).
                Select(t => t.Wordmark).
                ToArray();

            queryResult.Should().
                BeEquivalentTo(new[] { "ALPHA1", "BETA2" });
        }

        [Test]
        public void QueryRepository_Ignores_Whitespace_SearchTerm()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices0",
                sourceId: "testSourceId0",
                statusDetail: "testStatusDetail0",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                firstTestTrademark, 
                secondTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var whitespaceQueryResult = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "   ",
                ExactMatch = false
            }).Select(t => t.Wordmark).
            ToArray();

            whitespaceQueryResult.Should().
                BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }

        [Test]
        public void QueryRepository_ClassNumbers_NullOrEmpty_ReturnsAllTrademarks()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices0",
                sourceId: "testSourceId0",
                statusDetail: "testStatusDetail0",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.
                AddRange(firstTestTrademark, secondTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var nullClassQueryResult = trademarkRepository.Query(
                new TrademarkSearchFilter { ClassNumbers = null }).
                Select(t => t.Wordmark).
                ToArray();

            nullClassQueryResult.Should().
                BeEquivalentTo(new[] { "ALPHA", "BETA" });


            var emptyClassQueryResult = trademarkRepository.Query(
                new TrademarkSearchFilter { ClassNumbers = new int[0] }).
                Select(t => t.Wordmark).
                ToArray();

            emptyClassQueryResult.Should().
                BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }
    }
}
