using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class NumNormalizationTmRepositoryTests
    {
        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_RegistrationNumbers_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "us111abc",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_RegistrationNumbers_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_Serial_SourceId_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            trademarkEntity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "ir123456789",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_Serial_SourceId_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Bingo10",
                owner: "Michael Crafter",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            trademarkEntity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "4567",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "Bingo10" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_NullSides_DoNotThrow_And_DoNotMatch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dead and Divine",
                owner: "P Lower",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-999-XYZ",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (missingNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "XYZ 100",
                owner: "Frank Steward",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO);

            missingNumberEntity.SourceId = "SN-000-TEST";

            testDbContext.TrademarkRegistrations.AddRange(
                regNumberEntity, 
                missingNumberEntity);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "xyz",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "Dead and Divine" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_NoMatch_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AMZ",
                owner: "Liam Cooper",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                regNumberEntity, 
                regNumberEntity);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = true
            }).
            ToArray();

            queryResult.Should().BeEmpty();
        }

        [Test]
        public void QueryRepository_FilterByNumber_Match_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AMZ",
                owner: "Liam Cooper",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                regNumberEntity, 
                regNumberEntity);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "AMZ" });
        }
    }
}
