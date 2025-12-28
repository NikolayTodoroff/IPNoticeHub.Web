using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class NumNormalizationTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_RegistrationNumbers_ForExactSearch()
        {
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            entity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Bingo10",
                owner: "Michael Crafter",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            entity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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
