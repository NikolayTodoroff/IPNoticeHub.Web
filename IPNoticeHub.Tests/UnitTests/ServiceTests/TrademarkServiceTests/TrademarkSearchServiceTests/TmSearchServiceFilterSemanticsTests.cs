using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class TmSearchServiceFilterSemanticsTests
    {
        [Test]
        public async Task SearchAsync_WhenProviderFilterIsSet_ReturnsOnlyThatProvider()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmZZZ, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmAAA, 
                tmZZZ);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            pagedResult.Results.Single().Provider.Should().
                Be(DataProvider.USPTO);
        }

        [Test]
        public async Task SearchAsync_WhenStatusFilterIsSet_ReturnsOnlyThatStatus()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 35 });

            var (tmZZZ, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 9 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmAAA, 
                tmZZZ);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Status = TrademarkStatusCategory.Registered
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            pagedResult.Results.Single().Status.Should().
                Be(TrademarkStatusCategory.Registered);
        }

        [Test]
        public async Task SearchAsync_WhenClassFilterIsSet_ReturnsOnlyMarksContainingThatClass()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 35 });

            var (tmZZZ, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 9,11 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmAAA, 
                tmZZZ);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                ClassNumbers = new[] { 25 }
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            var targetTrademarkDTO = 
                pagedResult.Results.Single();

            targetTrademarkDTO.Wordmark.Should().
                Be("AAA");

            targetTrademarkDTO.Classes.Should().
                Contain(25);

            targetTrademarkDTO.Classes.Should().
                Contain(35);
        }

        [Test]
        public async Task SearchAsync_WhenMultipleFiltersSet_AppliesAllAsIntersection()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (mathchingAllEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (wrongProviderEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            var (wrongStatusEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (wrongClassEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DDD",
                owner: "Owner D",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2244432",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9 });

            testDbContext.TrademarkRegistrations.AddRange(
                mathchingAllEntity,
                wrongProviderEntity,
                wrongStatusEntity,
                wrongClassEntity);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            var targetTmEntity = 
                pagedResult.Results.Single();

            targetTmEntity.Wordmark.Should().
                Be("AAA");

            targetTmEntity.Provider.Should().
                Be(DataProvider.USPTO);

            targetTmEntity.Status.Should().
                Be(TrademarkStatusCategory.Registered);

            targetTmEntity.Classes.Should().
                Contain(25);
        }
    }
}
