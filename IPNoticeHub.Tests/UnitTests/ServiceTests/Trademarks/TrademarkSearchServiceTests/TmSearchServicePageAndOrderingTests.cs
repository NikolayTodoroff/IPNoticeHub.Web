using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
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
        public async Task SearchAsync_NoRestrictiveFilters_ReturnsOrderedPagedResults()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();
           
            var (trademarkEntityA1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner A1",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (trademarkEntityA2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner A2",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            var (trademarkEntityB, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "Owner B",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });


            testDbContext.TrademarkRegistrations.AddRange(trademarkEntityB, trademarkEntityA1, trademarkEntityA2);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository repo = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(repo);

            var filterDTO = new TrademarkFilterDTO
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false
            };

            var pagedResult = await service.SearchAsync(
                filter: filterDTO,
                currentPage: 1,
                resultsPerPage: 2,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(3);
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(2);

            pagedResult.Results.Should().HaveCount(2);
           
            var firstDTO = pagedResult.Results[0];
            var secondDTO = pagedResult.Results[1];

            firstDTO.Wordmark.Should().Be("ALPHA");
            secondDTO.Wordmark.Should().Be("ALPHA");

            firstDTO.PublicId.Should().NotBe(secondDTO.PublicId);
            firstDTO.Id.Should().NotBe(secondDTO.Id);

            firstDTO.SourceId.Should().NotBeNullOrWhiteSpace();
            firstDTO.Provider.Should().BeOneOf(DataProvider.USPTO, DataProvider.EUIPO);
            firstDTO.Classes.Should().NotBeNull();

        }
    }
}
