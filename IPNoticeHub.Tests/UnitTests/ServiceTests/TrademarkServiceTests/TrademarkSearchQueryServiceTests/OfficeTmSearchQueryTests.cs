using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class OfficeTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_WithOfficeUSPTO_FiltersOnlyUSPTO()
        {
            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark",
                owner: "Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var randomEntity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Random Test Wordmark",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

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
            queryResult.Single().Wordmark.Should().Be(entity.Wordmark);
        }
    }
}
