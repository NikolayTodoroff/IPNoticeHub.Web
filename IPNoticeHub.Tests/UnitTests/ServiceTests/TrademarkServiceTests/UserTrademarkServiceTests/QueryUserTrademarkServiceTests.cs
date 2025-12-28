using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class QueryUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task IsInCollectionAsync_WithAndWithoutSoftDeleted_TogglesAsExpected()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity2.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: tmEntity2.Id,
                cancellationToken: default);

            bool activeTrademarkExclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    tmEntity1.Id, 
                    includeSoftDeleted: false, 
                    cancellationToken: default);

            bool removedTrademarkExclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    tmEntity2.Id, 
                    includeSoftDeleted: false, 
                    cancellationToken: default);

            activeTrademarkExclSoftDelete.Should().BeTrue();
            removedTrademarkExclSoftDelete.Should().BeFalse();

            bool activeTrademarkInclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    tmEntity1.Id, 
                    includeSoftDeleted: true, 
                    cancellationToken: default);

            bool removedTrademarkInclSoftDelete = 
                await service.IsInCollectionAsync(
                    user.Id,
                    tmEntity2.Id, 
                    includeSoftDeleted: true, 
                    cancellationToken: default);

            activeTrademarkInclSoftDelete.Should().BeTrue();
            removedTrademarkInclSoftDelete.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_ReturnsPagedResults_WithDefaultOrdering()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity2.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity3.Id,
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
                new[] { tmEntity1.PublicId, 
                    tmEntity2.PublicId, 
                    tmEntity3.PublicId }.
                    ToHashSet());
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByWordmark_ReturnsCorrectOrderForBothDirections()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity2.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity3.Id,
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

            wordmarksOrderedAsc.Should().
                ContainInOrder("AAA", "BBB", "CCC");

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

            wordmarksOrderedDesc.Should().
                ContainInOrder("CCC", "BBB","AAA");
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByDateAdded_ReturnsCorrectOrderForBothDirections()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await Task.Delay(5);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity2.Id,
                cancellationToken: default);

            await Task.Delay(5);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity3.Id,
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

            datesAddedOrderedAsc.Should().
                ContainInOrder("AAA", "BBB", "CCC");

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

            wordmarksOrderedDesc.Should().
                ContainInOrder("CCC", "BBB", "AAA");
        }
    }
}
