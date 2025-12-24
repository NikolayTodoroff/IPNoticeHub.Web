using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    [TestFixture]
    public class OfficeTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_WithOfficeUSPTO_FiltersOnlyUSPTO()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Office = DataProvider.USPTO,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(
                    dto, 
                    CancellationToken.None);

            total.Should().Be(1);

            queryResult.Single().Wordmark.Should().
                Be("Anubis");
        }
    }
}
