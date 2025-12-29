using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class FilterTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenProviderFilterIsSet_ReturnsOnlyThatProvider()
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
               source: DataProvider.WIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark 3",
               owner: "Test Owner 3",
               goodsAndServices: "testGoodsAndSerices3",
               sourceId: "ZZZ456",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            result.Results.Single().Provider.
                Should().Be(DataProvider.USPTO);
        }

        [Test]
        public async Task SearchAsync_WhenStatusFilterIsSet_ReturnsOnlyThatStatus()
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
               source: DataProvider.WIPO);

            var entity3 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark 3",
               owner: "Test Owner 3",
               goodsAndServices: "testGoodsAndSerices3",
               sourceId: "ZZZ456",
               statusDetail: "Renew Deadline Passed",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Cancelled,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Status = TrademarkStatusCategory.Registered
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            result.Results.Single().Status.Should().
                Be(TrademarkStatusCategory.Registered);
        }

        [Test]
        public async Task SearchAsync_WhenClassFilterIsSet_ReturnsOnlyMarksContainingThatClass()
        {
            var (entity1, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark A",
               owner: "Correct Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "X123AZ",
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO,
               classNumbers: new[] { 15, 31 });

            var (entity2, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark B",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO,
               classNumbers: new[] { 35, 25 });

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                ClassNumbers = new[] { 25 }
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            var targetTrademarkDTO = result.Results.Single();

            targetTrademarkDTO.Wordmark.Should().Be(entity2.Wordmark);
            targetTrademarkDTO.Classes.Should().Contain(25);
            targetTrademarkDTO.Classes.Should().Contain(35);
        }

        [Test]
        public async Task SearchAsync_WhenMultipleFiltersSet_AppliesAllAsIntersection()
        {
            var (entity1, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark A",
               owner: "Correct Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "X123AZ",
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO,
               classNumbers: new[] { 15, 31 });

            var (entity2, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark B",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO,
               classNumbers: new[] { 35, 25 });

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            };

            var result = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            result.ResultsCount.Should().Be(1);
            result.Results.Should().ContainSingle();

            var targetTmEntity = result.Results.Single();

            targetTmEntity.Wordmark.Should().Be(entity2.Wordmark);
            targetTmEntity.Provider.Should().Be(DataProvider.USPTO);
            targetTmEntity.Status.Should().Be(TrademarkStatusCategory.Registered);
            targetTmEntity.Classes.Should().Contain(25);
        }
    }
}
