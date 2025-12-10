using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Application.Trademarks.DTOs;
using IPNoticeHub.Application.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class TmSearchServiceEdgeCaseTests
    {
        /// <summary>
        /// Section: TrademarkSearchService – Edge Cases For The SearchAsync Method.
        ///  - Verifies that the service returns all records when no search term is provided, ensuring default behavior is functional.
        ///  - Ensures that invalid values for currentPage and resultsPerPage are normalized to defaults, maintaining stability under improper inputs.
        ///  - Confirms that when no records match the search criteria, the result is empty, but metadata (e.g., CurrentPage, ResultsCountPerPage) remains correct.
        ///  - Each test uses an in-memory database to ensure isolation and reproducibility, with specific test data simulating real-world scenarios.
        ///  - Tests not only the result content but also the metadata (e.g., CurrentPage, ResultsCountPerPage, ResultsCount) to ensure accurate pagination information.
        /// </summary>
        [Test]
        public async Task SearchAsync_WhenSearchTermIsNullOrEmpty_ReturnsAllItems()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (tmZZZ, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(tmAAA, tmZZZ);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
            };

            var pagedResultDTO = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResultDTO.ResultsCount.Should().Be(2);

            var emptySearchTermFilterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "",
                ExactMatch = false,
            };

            var emptyPagedResultDTO = await service.SearchAsync(
                dto: emptySearchTermFilterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            emptyPagedResultDTO.ResultsCount.Should().Be(2);
        }

        [Test]
        public async Task SearchAsync_WhenPageOrSizeInvalid_NormalizesToDefaults()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO);

            var (tmEntity3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1, tmEntity2, tmEntity3);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            { 
                SearchBy = TrademarkSearchBy.Wordmark, 
                SearchTerm = null, 
                ExactMatch = false };

            var pagedResultDTO = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            pagedResultDTO.CurrentPage.Should().BeGreaterThan(0);
            pagedResultDTO.ResultsCountPerPage.Should().BeGreaterThan(0);
            pagedResultDTO.Results.Should().NotBeEmpty();
            pagedResultDTO.ResultsCount.Should().Be(3);
        }

        [Test]
        public async Task SearchAsync_WhenNoMatches_ReturnsEmptyResultsWithCorrectMetadata()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(tmEntity1);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "randomText",
                ExactMatch = false
            };

            var pagedResultDTO = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResultDTO.ResultsCount.Should().Be(0);
            pagedResultDTO.Results.Should().BeEmpty();
            pagedResultDTO.CurrentPage.Should().Be(1);
            pagedResultDTO.ResultsCountPerPage.Should().Be(10);
        }
    }
}
