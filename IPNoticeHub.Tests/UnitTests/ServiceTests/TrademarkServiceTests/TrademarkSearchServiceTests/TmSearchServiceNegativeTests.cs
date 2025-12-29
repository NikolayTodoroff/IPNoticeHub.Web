using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class TmSearchServiceNegativeTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenSearchTermIsNullOrEmpty_ReturnsAllItems()
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

            var nullTermDto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
            };

            var pagedResultDTO = 
                await service.SearchAsync(
                dto: nullTermDto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResultDTO.ResultsCount.Should().Be(3);

            var emptyTermDto = 
                new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "",
                ExactMatch = false,
            };

            var result = 
                await service.SearchAsync(
                dto: emptyTermDto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(3);
        }

        [Test]
        public async Task SearchAsync_WhenPageOrSizeInvalid_NormalizesToDefaults()
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

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            { 
                SearchBy = TrademarkSearchBy.Wordmark, 
                SearchTerm = null, 
                ExactMatch = false };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            result.CurrentPage.Should().BeGreaterThan(0);
            result.ResultsCountPerPage.Should().BeGreaterThan(0);
            result.Results.Should().NotBeEmpty();
            result.ResultsCount.Should().Be(2);
        }

        [Test]
        public async Task SearchAsync_WhenNoMatches_ReturnsEmptyResultsWithCorrectMetadata()
        {
            const string missingSearchTerm = "You Won't Find Me!";

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

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = missingSearchTerm,
                ExactMatch = false
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(0);
            result.Results.Should().BeEmpty();
            result.CurrentPage.Should().Be(1);
            result.ResultsCountPerPage.Should().Be(10);
        }
    }
}
