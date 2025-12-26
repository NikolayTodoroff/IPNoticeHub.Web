using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class SearchByOwnerTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchTrue_ReturnsOnlyExactOwner()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Owner A",
                ExactMatch = true
            };
          
            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            result.Results[0].Owner.
                Should().Be("Owner A");
        }

        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchFalse_ReturnsPartialMatches()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "owner",
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(3);

            result.Results.Select(
                r => r.Owner).
                Should().Contain(new[] { 
                    "Owner A", 
                    "Owner B",
                    "Owner C" });
        }
    }
}
