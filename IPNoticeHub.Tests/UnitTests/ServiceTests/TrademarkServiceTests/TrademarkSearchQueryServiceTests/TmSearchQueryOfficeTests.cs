using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Application.TrademarkSearch.DTOs;
using IPNoticeHub.Application.TrademarkSearch.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    [TestFixture]
    public class TmSearchQueryOfficeTests
    {
        [Test]
        public async Task SearchAsync_WithOfficeUSPTO_FiltersOnlyUSPTO()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.WIPO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Office = DataProvider.USPTO,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await queryService.SearchAsync(
                    query, 
                    CancellationToken.None);

            total.Should().
                Be(1);

            queryResult.Single().Wordmark.Should().
                Be("Anubis");
        }
    }
}
