using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class SearchByNumberTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesRegistrationNumber()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "1234567",
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = 
                pagedResult.Results.Single();

            result.Id.Should().Be(tmEntity1.Id);
            result.Wordmark.Should().Be("AAA");
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesSourceId()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "testSourceId1",
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = pagedResult.Results.Single();

            result.Id.Should().Be(tmEntity1.Id);
            result.Wordmark.Should().Be("AAA");
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchFalse_AllowsPartialMatches()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "1122",
                ExactMatch = false
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = pagedResult.Results.Single();

            result.Id.Should().Be(tmEntity3.Id);
            result.Wordmark.Should().Be("CCC");
        }
    }
}
