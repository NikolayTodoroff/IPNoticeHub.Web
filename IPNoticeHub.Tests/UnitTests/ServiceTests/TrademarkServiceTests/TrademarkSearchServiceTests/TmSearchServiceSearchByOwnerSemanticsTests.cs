using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Application.Trademarks.DTOs;
using IPNoticeHub.Application.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    /// <summary>
    /// Section: TrademarkSearchService - Search by Owner Semantics
    /// When ExactMatch is set to true, the search returns only the exact owner, ignoring case sensitivity.  
    /// When ExactMatch is set to false, the search includes partial matches, also ignoring case sensitivity.
    /// </summary>
    [TestFixture]
    public class TmSearchServiceSearchByOwnerSemanticsTests
    {
        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchTrue_ReturnsOnlyExactOwner()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1, tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Owner A",
                ExactMatch = true
            };
          
            var pagedResult = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();
            pagedResult.Results[0].Owner.Should().Be("Owner A");
        }

        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchFalse_ReturnsPartialMatches()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1, tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "owner",
                ExactMatch = false
            };

            var pagedResult = await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(2);
            pagedResult.Results.Select(r => r.Owner).Should().Contain(new[] { "Owner A", "Owner B" });
        }
    }
}
