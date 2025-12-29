using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.UserTrademarkRepositoryTests
{
    public class SoftRemoveUserTmRepositoryTests
    {
        [Test]
        public async Task SoftRemoveAsync_SoftDeletes_ReturnsTrue_WhenActiveLinkExists()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var user =
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Ruud G.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository =
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id,
                trademarkEntity.Id,
                CancellationToken.None);

            bool removedSuccessfully =
                await userTmRepository.SoftRemoveAsync(
                    user.Id,
                    trademarkEntity.Id,
                    CancellationToken.None);

            removedSuccessfully.Should().BeTrue();

            var userTmLink = await testDbContext.
                UserTrademarks.SingleAsync(
                ut => ut.ApplicationUserId == user.Id &&
                ut.TrademarkEntityId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task SoftRemoveAsync_ReturnsFalse_WhenLinkIsMissing_OrLinkIsAlreadyDeleted()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var user =
                InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days Later",
                owner: "Michael Owen",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository =
                new UserTrademarkRepository(testDbContext);

            bool removedMissingLink =
                await userTmRepository.SoftRemoveAsync(
                user.Id,
                trademarkEntity.Id,
                CancellationToken.None);

            removedMissingLink.Should().BeFalse();

            await userTmRepository.AddOrUndeleteAsync(
                user.Id,
                trademarkEntity.Id,
                CancellationToken.None);

            bool successfullyRemovedLink =
                await userTmRepository.SoftRemoveAsync(
                    user.Id,
                    trademarkEntity.Id,
                    CancellationToken.None);

            successfullyRemovedLink.Should().BeTrue();

            bool failedRemovedLink = await userTmRepository.SoftRemoveAsync(
                user.Id,
                trademarkEntity.Id,
                CancellationToken.None);

            failedRemovedLink.Should().BeFalse(
                "calling SoftRemove on an already-deleted link should be a no-op");
        }
    }
}
