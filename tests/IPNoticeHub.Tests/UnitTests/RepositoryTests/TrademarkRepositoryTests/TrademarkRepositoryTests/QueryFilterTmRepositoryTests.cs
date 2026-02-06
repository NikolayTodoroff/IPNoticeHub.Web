using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class QueryFilterTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_FilterByWordmark_ReturnsResults_ForExactMatch()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                entity1, 
                entity2);
            
            testDbContext.SaveChanges();

            string[]? wordmarkExactMatchResult = repository.Query(
                new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "Second Tower",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            wordmarkExactMatchResult.Should().Equal("Second Tower");
        }

        [Test]
        public void QueryRepository_FilterByWordmark_ReturnsResults_ForPartialMatch()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                entity1, 
                entity2);
            
            testDbContext.SaveChanges();

            string[]? wordmarkPartialMatchResult = repository.Query(
                new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "FIRST",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            wordmarkPartialMatchResult.Should().Equal("FIRSTWAVE");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ReturnsResults_ForExactMatch()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(
                entity1, 
                entity2);
            
            testDbContext.SaveChanges();

            IQueryable<TrademarkEntity>? exactOwnerMatches = 
                repository.Query(
                    new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "White Trades Inc",
                ExactMatch = true
            });

            exactOwnerMatches.Should().ContainSingle().Which.Owner.
                Should().Be("White Trades Inc");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ReturnsResults_ForPartialMatch()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(
                entity1, 
                entity2);
            
            testDbContext.SaveChanges();

            IQueryable<TrademarkEntity>? partialOwnerMatches = 
                repository.Query(
                    new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Black",
                ExactMatch = false
            });

            partialOwnerMatches.Should().ContainSingle().
                Which.Owner.Should().Be("Black Company LLC");
        }

        [Test]
        public void QueryRepository_FilterByNumber_RegistrationNumber_ReturnsResults_ForExactMatch()
        {
            var (registryNumberTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (serialNumberTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "OMEGA",
                owner: "B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            testDbContext.TrademarkRegistrations.AddRange(
                registryNumberTrademark, 
                serialNumberTrademark);
            
            testDbContext.SaveChanges();

            var queryResults = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "us-111-abc",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResults.Should().Equal("ALPHA");
        }

        [Test]
        public void QueryRepository_FilterByNumber_RegistrationNumber_ReturnsResults_ForPartialMatch()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Alex T",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Beta",
                owner: "George Orwell",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-201-aBc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity3, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA",
                owner: "George B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-999-ZZZ",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                entity1, 
                entity2,
                entity3);

            testDbContext.SaveChanges();

            var queryResults = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResults.Should().BeEquivalentTo(new[] { "ALPHA", "Beta" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_SerialNumber_SourceId_ReturnsResults_ForExactMatch()
        {
            var (serialNumberTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dead End",
                owner: "Elton John",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned, 
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            var (registryNumberTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BB12",
                owner: "Osho",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                serialNumberTrademark, 
                registryNumberTrademark);
            
            testDbContext.SaveChanges();

            var queryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "sn-222-xyz",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().Equal("Dead End");
        }

        [Test]
        public void QueryRepository_FilterByNumber_SerialNumber_SourceId_ReturnsResults_ForPartialMatch()
        {
            var (serialNumberTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "S&P500",
                owner: "B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            var (randomTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Delta Force", 
                owner: "D",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US-333-QQQ",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                serialNumberTrademark, 
                randomTrademark);
            
            testDbContext.SaveChanges();

            var queryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "222",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "S&P500" });
        }

        [Test]
        public void QueryRepository_WithDefaultFilter_ReturnsAllTrademarks()
        {
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

            var repository =
                new TrademarkRepository(testDbContext);

            var queryResult = repository.Query(
                new TrademarkSearchFilter()).
                Select(t => t.Wordmark).
                ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "ALPHA1", "BETA2" });
        }
    }
}
