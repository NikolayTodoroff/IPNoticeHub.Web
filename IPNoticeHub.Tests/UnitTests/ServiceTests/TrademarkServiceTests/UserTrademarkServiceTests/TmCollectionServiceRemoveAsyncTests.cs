using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    /// <summary>
    /// Section: TrademarkCollectionService - RemoveAsync(Soft-Delete) behavior.
    /// - Verifies that when a trademark is in the user's collection, calling RemoveAsync performs a soft delete by setting IsDeleted to true.
    /// - Ensures that the soft-deleted link is not considered active in the collection.
    /// - Confirms that the soft-deleted link does not appear in the collection when includeSoftDeleted is false.
    /// </summary>
    [TestFixture]
    public class TmCollectionServiceRemoveAsyncTests
    {
        [Test]
        public async Task RemoveAsync_WhenInCollection_PerformsSoftDeleteOnly()
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

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            var softDeletedLinks = testDbContext.UserTrademarks.Where(
                x => x.UserId == user.Id && x.TrademarkId == trademarkEntity.Id).ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task RemoveAsync_WhenInCollection_SoftDeletedLinkNotConsideredActive()
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

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            bool isLinkActive = await service.IsInCollectionAsync(
                user.Id, trademarkEntity.Id, includeSoftDeleted: false, cancellationToken: default);

            isLinkActive.Should().BeFalse();

            bool linkExists = await service.IsInCollectionAsync(
                user.Id, trademarkEntity.Id, includeSoftDeleted: false, cancellationToken: default);

            linkExists.Should().BeFalse();
        }
    }
}
