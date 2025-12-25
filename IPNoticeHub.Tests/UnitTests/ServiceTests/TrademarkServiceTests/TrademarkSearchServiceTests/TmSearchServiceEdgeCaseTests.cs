using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class TmSearchServiceEdgeCaseTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenSearchTermIsNullOrEmpty_ReturnsAllItems()
        {
            var nullTermDto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
            };

            var pagedResultDTO = 
                await service.SearchAsync(
                dto: nullTermDto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResultDTO.ResultsCount.Should().Be(3);

            var emptyTermDto = 
                new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "",
                ExactMatch = false,
            };

            var result = 
                await service.SearchAsync(
                dto: emptyTermDto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(3);
        }

        [Test]
        public async Task SearchAsync_WhenPageOrSizeInvalid_NormalizesToDefaults()
        {
            var dto = new TrademarkFilterDto
            { 
                SearchBy = TrademarkSearchBy.Wordmark, 
                SearchTerm = null, 
                ExactMatch = false };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            result.CurrentPage.Should().BeGreaterThan(0);
            result.ResultsCountPerPage.Should().BeGreaterThan(0);
            result.Results.Should().NotBeEmpty();
            result.ResultsCount.Should().Be(3);
        }

        [Test]
        public async Task SearchAsync_WhenNoMatches_ReturnsEmptyResultsWithCorrectMetadata()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "randomText",
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(0);
            result.Results.Should().BeEmpty();
            result.CurrentPage.Should().Be(1);
            result.ResultsCountPerPage.Should().Be(10);
        }
    }
}
