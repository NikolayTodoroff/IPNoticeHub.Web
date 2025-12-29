using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class SearchByOwnerTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchTrue_ReturnsOnlyExactOwner()
        {
            const string expectedOwner = "OwnerX";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Wordmark A",
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
               owner: expectedOwner,
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
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = expectedOwner,
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
                Should().Be(entity3.Owner);
        }

        [Test]
        public async Task SearchAsync_WhenOwnerExactMatchFalse_ReturnsPartialMatches()
        {
            const string expectedOwner = "OwnerX";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Wordmark A",
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
               owner: $"Test {expectedOwner} 2",
               goodsAndServices: "testGoodsAndSerices B",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark C",
               owner: $"Test {expectedOwner} 3",
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
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = expectedOwner,
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(2);

            result.Results.Select(
                r => r.Owner).
                Should().Contain(new[] { entity2.Owner, entity3.Owner });
        }
    }
}
