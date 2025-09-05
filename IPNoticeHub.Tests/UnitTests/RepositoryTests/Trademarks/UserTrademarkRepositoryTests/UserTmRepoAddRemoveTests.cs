using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    /// <summary>
    /// Section: Add - Undelete / Soft Removal semantics
    /// Ensures that AddOrUndeleteAsync:
    /// - Inserts a new link when none exists (active, not deleted).
    /// - Does not create duplicates on repeated calls when already active (idempotent).
    /// - When a soft-deleted link exists, it flips IsDeleted=false and refreshes
    /// Ensures that SoftRemove:
    /// - Returns true and flips IsDeleted=true when an active link exists.
    /// - Returns false when the link is missing or already soft-deleted (idempotent).
    /// </summary>
    [TestFixture]
    public class UserTmRepoAddRemoveTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_ShouldInsertNewLink_WhenLinkIsMissing()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            var userTmLink = await testDbContext.UserTrademarks
                .Include(ut => ut.TrademarkRegistration)
                .SingleAsync(ut => ut.ApplicationUserId == user.Id && ut.TrademarkRegistrationId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeFalse();
            userTmLink.AddedToWatchlist.Should().BeTrue();
            userTmLink.DateAdded.Should().BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));
        }

        [Test]
        public async Task SoftRemoveAsync_SoftDeletes_ReturnsTrue_WhenActiveLinkExists()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            bool removedSuccessfully = await userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            removedSuccessfully.Should().BeTrue();

            var userTmLink = await testDbContext.UserTrademarks
                .SingleAsync(ut => ut.ApplicationUserId == user.Id && ut.TrademarkRegistrationId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task SoftRemoveAsync_ReturnsFalse_WhenLinkIsMissing_OrLinkIsAlreadyDeleted()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            bool removedMissingLink = await userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, CancellationToken.None);
            removedMissingLink.Should().BeFalse();

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            bool successfullyRemovedLink = await userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, CancellationToken.None);
            successfullyRemovedLink.Should().BeTrue();

            bool failedRemovedLink = await userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, CancellationToken.None);
            failedRemovedLink.Should().BeFalse("calling SoftRemove on an already-deleted link should be a no-op");
        }
    }
}
