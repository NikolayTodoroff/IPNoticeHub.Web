using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
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
    /// - Calling AddOrUndeleteAsync again on an already-active link does NOT create duplicates
    ///   and does NOT modify DateAdded/AddedToWatchlist.
    /// - When link is soft-deleted, calling AddOrUndeleteAsync sets IsDeleted=false and 
    /// refreshes AddedToWatchlist=true and DateAdded to "now".
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
                wordmark: "Spiritfarer",
                owner: "Obama B.L.",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            var userTmLink = await testDbContext.UserTrademarks
                .Include(ut => ut.Trademark)
                .SingleAsync(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeFalse();
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
                owner: "Ruud G.",
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
                .SingleAsync(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task SoftRemoveAsync_ReturnsFalse_WhenLinkIsMissing_OrLinkIsAlreadyDeleted()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days Later",
                owner: "Michael Owen",
                regNumber: "1234567",
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

        [Test]
        public async Task AddOrUndeleteAsync_IsIdempotent_WhenAlreadyActive_DoesNotChangeDateAdded()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Swinging Back",
                owner: "The Chosen One",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered, 
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            var userTmLink = await testDbContext.UserTrademarks
                .SingleAsync(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id);

            DateTime initialDateAdded = userTmLink.DateAdded;

            // Introduce a small delay to ensure timestamp precision and detect unintended updates
            await Task.Delay(50);

            // Attempting to add the same link again — should have no effect on an already active link 
            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            var queryLinksResult = await testDbContext.UserTrademarks
                .Where(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id)
                .ToListAsync();

            queryLinksResult.Count.Should().Be(1);

            var singleQueryResult = queryLinksResult.Single();
            singleQueryResult.IsDeleted.Should().BeFalse();
            singleQueryResult.DateAdded.Should().Be(initialDateAdded);
        }

        [Test]
        public async Task AddOrUndeleteAsync_UndeletesAndRefreshesFlagsAndDate()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Chimaira",
                owner: "Default User",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            var userTmLink = await testDbContext.UserTrademarks
                .SingleAsync(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id);

            DateTime initialDateAdded = userTmLink.DateAdded;
            userTmLink.IsDeleted.Should().BeFalse();

            await userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            // Introduce a small delay to ensure timestamp precision and detect unintended updates
            await Task.Delay(50);

            // Triggering Undelete operation via AddOrUndeleteAsync 
            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, CancellationToken.None);

            UserTrademark? undeletedLink = await testDbContext.UserTrademarks
                .SingleAsync(ut => ut.UserId == user.Id && ut.TrademarkId == trademarkEntity.Id);

            undeletedLink.IsDeleted.Should().BeFalse();
            undeletedLink.DateAdded.Should().BeAfter(initialDateAdded);
        }

    }
}
