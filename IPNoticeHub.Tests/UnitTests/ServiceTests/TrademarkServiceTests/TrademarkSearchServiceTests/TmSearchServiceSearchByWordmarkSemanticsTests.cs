using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class TmSearchServiceSearchByWordmarkSemanticsTests
    {
        [Test]
        public async Task SearchAsync_WhenExactMatchTrue_ReturnsOnlyExactWordmark()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Here & Now",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Gone With The Wind",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "here & now",
                ExactMatch = true
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

            pagedResult.Results[0].Wordmark.Should().
                Be("Here & Now");
        }

        [Test]
        public async Task SearchAsync_WhenExactMatchFalse_ReturnsPartialMatches()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Gone Forever",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Gone With The Wind",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "gone",
                ExactMatch = false
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(2);

            pagedResult.Results.
                Select(r => r.Wordmark).Should().
                Contain(new[] { "Gone Forever", "Gone With The Wind" });
        }
    }
}
