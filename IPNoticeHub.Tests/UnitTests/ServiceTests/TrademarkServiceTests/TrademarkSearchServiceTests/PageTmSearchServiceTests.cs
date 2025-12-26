using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class PageTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenNoFiltersApplied_ReturnsPagedResultsMetadata()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
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
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 2,
                cancellationToken: default);

            result.Results.Should().HaveCount(2);
            result.Results[0].Wordmark.Should().Be("AAA");
            result.Results[1].Wordmark.Should(). Be("BBB");
        }
    }
}
