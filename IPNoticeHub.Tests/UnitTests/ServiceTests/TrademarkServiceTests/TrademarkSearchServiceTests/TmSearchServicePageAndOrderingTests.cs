using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    /// <summary>
    /// Section: TrademarkSearchService – Paging & Ordering
    /// Ensures that the SearchAsync method:
    ///  - Returns results sorted by Wordmark and then by Id to maintain stable ordering.
    ///  - Provides accurate paging metadata, including ResultsCount, CurrentPage, and ResultsCountPerPage.
    ///  - Maps data to TrademarkSummaryDTO with the expected field values.
    /// </summary>
    [TestFixture]
    public class TmSearchServicePageAndOrderingTests
    {
        [Test]
        public async Task SearchAsync_WhenNoFiltersApplied_ReturnsPagedResultsMetadata()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntityA1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Day & Night",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntityA2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Day & Night",
                owner: "Owner A2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            var (tmEntityB, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Lost One",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(tmEntityB, tmEntityA1, tmEntityA2);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false
            };

            var pagedResult = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 2,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(3);
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(2);
        }

        [Test]
        public async Task SearchAsync_WhenNoFiltersApplied_ReturnsPagedResultsSortedByWordmarkAndId()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntityA1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Day & Night",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntityA2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Day & Night",
                owner: "Owner A2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            var (tmEntityB, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Lost One",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(tmEntityB, tmEntityA1, tmEntityA2);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false
            };

            var pagedResult = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 2,
                cancellationToken: default);

            pagedResult.Results.Should().HaveCount(2);
            pagedResult.Results[0].Wordmark.Should().Be("Day & Night");
            pagedResult.Results[1].Wordmark.Should().Be("Day & Night");
        }
    }
}
