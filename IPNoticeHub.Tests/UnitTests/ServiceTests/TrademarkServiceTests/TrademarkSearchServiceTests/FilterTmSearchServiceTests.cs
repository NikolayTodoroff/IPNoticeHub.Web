using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class FilterTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenProviderFilterIsSet_ReturnsOnlyThatProvider()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            result.Results.Single().Provider.
                Should().Be(DataProvider.USPTO);
        }

        [Test]
        public async Task SearchAsync_WhenStatusFilterIsSet_ReturnsOnlyThatStatus()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Status = TrademarkStatusCategory.Registered
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            result.Results.Single().Status.Should().
                Be(TrademarkStatusCategory.Registered);
        }

        [Test]
        public async Task SearchAsync_WhenClassFilterIsSet_ReturnsOnlyMarksContainingThatClass()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                ClassNumbers = new[] { 25 }
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            var targetTrademarkDTO = 
                result.Results.Single();

            targetTrademarkDTO.Wordmark.
                Should().Be("AAA");

            targetTrademarkDTO.Classes.
                Should().Contain(25);

            targetTrademarkDTO.Classes.
                Should().Contain(35);
        }

        [Test]
        public async Task SearchAsync_WhenMultipleFiltersSet_AppliesAllAsIntersection()
        {
            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            var targetTmEntity = 
                result.Results.Single();

            targetTmEntity.Wordmark.Should().
                Be("AAA");

            targetTmEntity.Provider.
                Should().Be(DataProvider.USPTO);

            targetTmEntity.Status.
                Should().Be(TrademarkStatusCategory.Registered);

            targetTmEntity.Classes.
                Should().Contain(25);
        }
    }
}
