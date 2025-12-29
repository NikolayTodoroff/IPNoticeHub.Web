using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class PagingTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_Paging_ReturnsSecondItemOnPage2_AndKeepsTotal()
        {
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
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2,entity3);
            await testDbContext.SaveChangesAsync();

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
                Should().Be(entity3.RegistrationNumber);
        }

        [Test]
        public async Task SearchAsync_Paging_OutOfRange_ReturnsEmpty_AndKeepsTotal()
        {
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
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

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
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "A1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark B",
               owner: "Test Owner B",
               goodsAndServices: "testGoodsAndSerices B",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "B7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark C",
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "C2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

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
                Should().ContainInOrder(
                entity1.RegistrationNumber,
                entity2.RegistrationNumber,
                entity3.RegistrationNumber);
        }

        [Test]
        public async Task SearchAsync_WhenPageIsZero_TreatsAsPage1_ReturnsFirstSlice()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "A1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark B",
               owner: "Test Owner B",
               goodsAndServices: "testGoodsAndSerices B",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "B7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark C",
               owner: "Test Owner C",
               goodsAndServices: "testGoodsAndSerices C",
               sourceId: "G123SQ",
               statusDetail: "Awaiting Approval",
               regNumber: "C2233441",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

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
                Should().Be(entity1.RegistrationNumber);
        }
    }
}
