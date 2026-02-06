using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.TrademarkSearchServiceTests
{
    public class SearchByWordmarkTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenExactMatchTrue_ReturnsOnlyExactWordmark()
        {
            const string expectedSearchTerm = "Test Owner A : Find Me!";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: expectedSearchTerm,
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark B",
               owner: "Test Owner B",
               goodsAndServices: "testGoodsAndSerices B",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark C",
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = expectedSearchTerm,
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

            result.Results[0].Wordmark.
                Should().Be(entity1.Wordmark);
        }

        [Test]
        public async Task SearchAsync_WhenExactMatchFalse_ReturnsPartialMatches()
        {
            const string expectedSearchTerm = "Find Me";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: $"{expectedSearchTerm} if you can!",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark B",
               owner: "Test Owner B",
               goodsAndServices: "testGoodsAndSerices B",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark C",
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = expectedSearchTerm,
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);

            result.Results[0].Wordmark.
                Should().Be(entity1.Wordmark);
        }
    }
}
