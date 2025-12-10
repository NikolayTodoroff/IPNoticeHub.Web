using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    /// <summary>
    /// Section: TrademarkCollectionService - Queries behavior.
    /// - Verifies that trademarks in the user's collection are correctly identified as active or soft-deleted based on the includeSoftDeleted flag.
    /// - Ensures that soft-deleted trademarks are excluded from the collection when includeSoftDeleted is false.
    /// - Confirms that soft-deleted trademarks are included in the collection when includeSoftDeleted is true.
    /// - Validates that GetUserCollectionAsync returns paginated results with correct default ordering.
    /// - Tests sorting functionality for GetUserCollectionAsync by wordmark and date added in both ascending and descending orders.
    /// </summary>
    [TestFixture]
    public class TmCollectionServiceQueryTests
    {
        [Test]
        public async Task IsInCollectionAsync_WithAndWithoutSoftDeleted_TogglesAsExpected()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (activeTmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (removedTmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.AddRange(activeTmEntity,removedTmEntity);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: activeTmEntity.Id,
                cancellationToken: default);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: removedTmEntity.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: removedTmEntity.Id,
                cancellationToken: default);

            bool activeTmPresentWithSoftDeleteExcl = await service.IsInCollectionAsync(user.Id, activeTmEntity.Id, includeSoftDeleted: false, cancellationToken: default);
            bool removedTmPresentWithSoftDeleteExcl = await service.IsInCollectionAsync(user.Id, removedTmEntity.Id, includeSoftDeleted: false, cancellationToken: default);

            activeTmPresentWithSoftDeleteExcl.Should().BeTrue();
            removedTmPresentWithSoftDeleteExcl.Should().BeFalse();

            bool activeTmPresentWithSoftDeleteIncl = await service.IsInCollectionAsync(user.Id, activeTmEntity.Id, includeSoftDeleted: true, cancellationToken: default);
            bool removedTmPresentWithSoftDeleteIncl = await service.IsInCollectionAsync(user.Id, removedTmEntity.Id, includeSoftDeleted: true, cancellationToken: default);

            activeTmPresentWithSoftDeleteIncl.Should().BeTrue();
            removedTmPresentWithSoftDeleteIncl.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_ReturnsPagedResults_WithDefaultOrdering()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4433221",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 15 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1,tmEntity2,tmEntity3);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

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

            var pagedResult = await service.GetUserCollectionAsync(
                userId:user.Id, currentPage: 1, resultsPerPage: 2, cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(3);
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(2);

            pagedResult.Results.Should().HaveCount(2);
            var returnedPublicIds = pagedResult.Results.Select(r => r.PublicId).ToHashSet();
            returnedPublicIds.Should().BeSubsetOf(new[] { tmEntity1.PublicId, tmEntity2.PublicId, tmEntity3.PublicId }.ToHashSet());
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByWordmark_ReturnsCorrectOrderForBothDirections()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4433221",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 15 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1, tmEntity2, tmEntity3);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

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

            var wordmarkAscPagedResult = await service.GetUserCollectionAsync(
                userId: user.Id, currentPage: 1, resultsPerPage: 10, cancellationToken: default, sortBy: CollectionSortBy.WordmarkAsc);

            var wordmarksOrderedAsc = wordmarkAscPagedResult.Results.Select(r => r.Wordmark).ToList();
            wordmarksOrderedAsc.Should().ContainInOrder("AAA", "BBB", "CCC");


            var wordmarkDescPagedResult = await service.GetUserCollectionAsync(
                userId: user.Id, currentPage: 1, resultsPerPage: 10, cancellationToken: default, sortBy: CollectionSortBy.WordmarkDesc);

            var wordmarksOrderedDesc = wordmarkDescPagedResult.Results.Select(r => r.Wordmark).ToList();
            wordmarksOrderedDesc.Should().ContainInOrder("CCC", "BBB","AAA");
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenSortedByDateAdded_ReturnsCorrectOrderForBothDirections()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BBB",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "CCC",
                owner: "Owner C",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4433221",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 15 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.AddRange(tmEntity1, tmEntity2, tmEntity3);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

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

            var dateAddedAscPagedResult = await service.GetUserCollectionAsync(
                userId: user.Id, currentPage: 1, resultsPerPage: 10, cancellationToken: default, sortBy: CollectionSortBy.DateAddedAsc);

            var datesAddedOrderedAsc = dateAddedAscPagedResult.Results.Select(r => r.Wordmark).ToList();
            datesAddedOrderedAsc.Should().ContainInOrder("AAA", "BBB", "CCC");


            var dateAddedDescPagedResult = await service.GetUserCollectionAsync(
                userId: user.Id, currentPage: 1, resultsPerPage: 10, cancellationToken: default, sortBy: CollectionSortBy.DateAddedDesc);

            var wordmarksOrderedDesc = dateAddedDescPagedResult.Results.Select(r => r.Wordmark).ToList();
            wordmarksOrderedDesc.Should().ContainInOrder("CCC", "BBB", "AAA");

        }
    }
}
