using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.TrademarkSearchServiceTests
{
    public class PageTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenNoFiltersApplied_ReturnsPagedResultsMetadata()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark 1",
                owner: "Test Owner 1",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "2346532",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark 2",
               owner: "Test Owner 2",
               goodsAndServices: "testGoodsAndSerices2",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark 3",
               owner: "Test Owner 3",
               goodsAndServices: "testGoodsAndSerices3",
               sourceId: "ZZZ456",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2,entity3);
            await testDbContext.SaveChangesAsync();

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
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "2346532",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark B",
               owner: "Test Owner B",
               goodsAndServices: "testGoodsAndSerices2",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark C",
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices3",
               sourceId: "ZZZ456",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

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
            result.Results[0].Wordmark.Should().Be(entity1.Wordmark);
            result.Results[1].Wordmark.Should(). Be(entity2.Wordmark);
        }
    }
}
