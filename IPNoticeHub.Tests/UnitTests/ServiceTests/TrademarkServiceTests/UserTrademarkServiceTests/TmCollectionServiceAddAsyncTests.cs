using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Application.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    /// <summary>
    /// Section: TrademarkCollectionService - AddAsync behaviour.
    /// - Verifies that when a trademark is not already in the user's collection, it is added successfully,
    /// and subsequent checks confirm its presence in the collection.
    /// - Verifies that if a soft-deleted link exists, AddAsync reactivates the same link (IsDeleted=false).
    /// </summary>
    [TestFixture]
    public class TmCollectionServiceAddAsyncTests
    {
        [Test]
        public async Task AddAsync_WhenNotInCollection_AddsLink_ThenIsInCollectionReturnsTrue()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            bool linkedInCollection = await service.IsInCollectionAsync(
                user.Id, trademarkEntity.Id, includeSoftDeleted: false, cancellationToken: default);

            linkedInCollection.Should().BeTrue();

            var existingLink = testDbContext.UserTrademarks.Where(x => x.UserId == user.Id && x.TrademarkId == trademarkEntity.Id).ToList();
            existingLink.Should().HaveCount(1);
            existingLink[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddAsync_WhenPreviouslySoftDeleted_UndeletesExistingLink()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            // Create a link, then soft-delete it, assert that the link exists but is soft-deleted
            await service.AddAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            var softDeletedLinks = testDbContext.UserTrademarks.Where(
                x => x.UserId == user.Id && x.TrademarkId == trademarkEntity.Id).ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();

            // AddAsync again should "undelete" the same link, assert that the link is active again
            await service.AddAsync(
               userId: user.Id,
               trademarkId: trademarkEntity.Id,
               cancellationToken: default);

            var unDeletedLinks = testDbContext.UserTrademarks.Where(
                x => x.UserId == user.Id && x.TrademarkId == trademarkEntity.Id).ToList();

            unDeletedLinks.Should().HaveCount(1);
            unDeletedLinks[index: 0].IsDeleted.Should().BeFalse();


            bool linkedInCollection = await service.IsInCollectionAsync(
                user.Id, trademarkEntity.Id, includeSoftDeleted: false, cancellationToken: default);

            linkedInCollection.Should().BeTrue();
        }
    }
}
