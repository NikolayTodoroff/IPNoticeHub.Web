using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class PagingTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_Paging_ReturnsSecondItemOnPage2_AndKeepsTotal()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 2,
                PageSize = 1
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(3);

            queryResult.Single().RegistrationNumber.
                Should().Be("1234567");
        }

        [Test]
        public async Task SearchAsync_Paging_OutOfRange_ReturnsEmpty_AndKeepsTotal()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 3,
                PageSize = 2
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(3);

            queryResult.Should().BeEmpty();
        }

        [Test]
        public async Task SearchAsync_WithEmptyQueryAndNoFilters_ReturnsAll_OrderedByRegistrationNumber()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = null,
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(3);

            queryResult.Should().HaveCount(3);

            queryResult.Select(q => q.RegistrationNumber).
                Should().ContainInOrder("1234512", "1234567", "3355442");
        }

        [Test]
        public async Task SearchAsync_WhenPageIsZero_TreatsAsPage1_ReturnsFirstSlice()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 0,
                PageSize = 1
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(3);

            queryResult.Should().ContainSingle();

            queryResult.Single().RegistrationNumber.
                Should().Be("1234512");
        }
    }
}
