using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.UserTrademarkServiceTests
{
    public class QueryUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task IsInCollectionAsync_WithAndWithoutSoftDeleted_TogglesAsExpected()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark B",
                owner: "Test Owner B",
                goodsAndServices: "testGoodsAndSerices B",
                sourceId: "C176AQ",
                statusDetail: "Successfully Registered",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1,entity2);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity2.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: entity2.Id,
                cancellationToken: default);

            bool activeTrademarkExclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    entity1.Id, 
                    includeSoftDeleted: false, 
                    cancellationToken: default);

            bool removedTrademarkExclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    entity2.Id, 
                    includeSoftDeleted: false, 
                    cancellationToken: default);

            activeTrademarkExclSoftDelete.Should().BeTrue();
            removedTrademarkExclSoftDelete.Should().BeFalse();

            bool activeTrademarkInclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    entity1.Id, 
                    includeSoftDeleted: true, 
                    cancellationToken: default);

            bool removedTrademarkInclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    entity2.Id, 
                    includeSoftDeleted: true, 
                    cancellationToken: default);

            activeTrademarkInclSoftDelete.Should().BeTrue();
            removedTrademarkInclSoftDelete.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_ReturnsPagedResults_WithDefaultOrdering()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark B",
                owner: "Test Owner B",
                goodsAndServices: "testGoodsAndSerices B",
                sourceId: "C176AQ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity3 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark C",
                owner: "Test Owner C",
                goodsAndServices: "testGoodsAndSerices C",
                sourceId: "F876EQ",
                statusDetail: "Successfully Registered",
                regNumber: "22334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1,entity2,entity3);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity2.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity3.Id,
                cancellationToken: default);

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId:user.Id, 
                currentPage: 1, 
                resultsPerPage: 2, 
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(3);
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(2);

            pagedResult.Results.Should().HaveCount(2);

            var publicIds = 
                pagedResult.Results.Select(
                    r => r.PublicId).ToHashSet();

            publicIds.Should().BeSubsetOf(
                new[] { entity1.PublicId,
                    entity2.PublicId,
                    entity3.PublicId }.
                    ToHashSet());
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByWordmark_ReturnsCorrectOrderForBothDirections()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark B",
                owner: "Test Owner B",
                goodsAndServices: "testGoodsAndSerices B",
                sourceId: "C176AQ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity3 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark C",
                owner: "Test Owner C",
                goodsAndServices: "testGoodsAndSerices C",
                sourceId: "F876EQ",
                statusDetail: "Successfully Registered",
                regNumber: "22334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity2.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity3.Id,
                cancellationToken: default);

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id, 
                currentPage: 1, 
                resultsPerPage: 10, 
                cancellationToken: default, 
                sortBy: CollectionSortBy.WordmarkAsc);

            var wordmarksOrderedAsc = 
                pagedResult.Results.Select(
                    r => r.Wordmark).ToList();

            wordmarksOrderedAsc.Should().ContainInOrder(
                entity1.Wordmark, 
                entity2.Wordmark, 
                entity3.Wordmark);

            var wordmarkDescPagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id, 
                currentPage: 1, 
                resultsPerPage: 10, 
                cancellationToken: default, 
                sortBy: CollectionSortBy.WordmarkDesc);

            var wordmarksOrderedDesc = 
                wordmarkDescPagedResult.Results.Select(
                    r => r.Wordmark).ToList();

            wordmarksOrderedDesc.Should().ContainInOrder(
                entity3.Wordmark, 
                entity2.Wordmark, 
                entity1.Wordmark);
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByDateAdded_ReturnsCorrectOrderForBothDirections()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark B",
                owner: "Test Owner B",
                goodsAndServices: "testGoodsAndSerices B",
                sourceId: "C176AQ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity3 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark C",
                owner: "Test Owner C",
                goodsAndServices: "testGoodsAndSerices C",
                sourceId: "F876EQ",
                statusDetail: "Successfully Registered",
                regNumber: "22334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await Task.Delay(5);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity2.Id,
                cancellationToken: default);

            await Task.Delay(5);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity3.Id,
                cancellationToken: default);

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id, 
                currentPage: 1, 
                resultsPerPage: 10, 
                cancellationToken: default, 
                sortBy: CollectionSortBy.DateAddedAsc);

            var datesAddedOrderedAsc = 
                pagedResult.Results.Select(
                    r => r.Wordmark).ToList();

            datesAddedOrderedAsc.Should().ContainInOrder(
                entity1.Wordmark, 
                entity2.Wordmark, 
                entity3.Wordmark);

            var dateAddedDescPagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id, 
                currentPage: 1, 
                resultsPerPage: 10, 
                cancellationToken: default, 
                sortBy: CollectionSortBy.DateAddedDesc);

            var wordmarksOrderedDesc = 
                dateAddedDescPagedResult.Results.Select(
                    r => r.Wordmark).ToList();

            wordmarksOrderedDesc.Should().ContainInOrder(
                entity3.Wordmark, 
                entity2.Wordmark, 
                entity1.Wordmark);
        }
    }
}
