using FluentAssertions;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    /// <summary>
    /// Section: UserCopyrightRepository Edge Cases
    /// - Ensures AddOrUndeleteAsync does not create duplicate rows for already active links.
    /// - Confirms SoftRemoveAsync returns false when the link is missing.
    /// - Verifies SoftRemoveAsync returns false for links already soft-deleted.
    /// - Validates IsLinkedAsync returns false for non-existent links, regardless of the includeSoftDeleted flag.
    /// - Ensures QueryUserCollection and QueryUserLinks return empty results for users without links.
    /// - Confirms multi-user isolation, ensuring results are scoped to individual users.
    /// </summary>
    [TestFixture]
    public class UserCopyrightRepoEdgeCasesTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_WhenAlreadyActive_DoesNotDuplicateRow()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "user7", UserName = "user7", Email = "u7@test" };
            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright("TX-7", "copyrightRegG");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(user.Id, copyrightEntity.Id);
            await copyrightRepo.AddOrUndeleteAsync(user.Id, copyrightEntity.Id);

            var links = await testDbContext.UserCopyrights
                .Where(x => x.ApplicationUserId == user.Id && x.CopyrightRegistrationId == copyrightEntity.Id)
                .ToListAsync();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenMissingLink_ReturnsFalse()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u8", UserName = "user8", Email = "u8@test" };
            var cp = InMemoryDbContextFactory.CreateCopyright("TX-8", "H");
            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(cp);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            var removed = await copyrightRepo.SoftRemoveAsync(user.Id, cp.Id);

            removed.Should().BeFalse();

            (await testDbContext.UserCopyrights.Where(
                x => x.ApplicationUserId == user.Id && x.CopyrightRegistrationId == cp.Id).
                ToListAsync()).Should().BeEmpty();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenAlreadySoftDeleted_ReturnsFalse()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u9", UserName = "user9", Email = "u9@test" };
            var cp = InMemoryDbContextFactory.CreateCopyright("TX-9", "I");
            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(cp);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(user.Id, cp.Id);
            await copyrightRepo.SoftRemoveAsync(user.Id, cp.Id);

            var removedAgain = await copyrightRepo.SoftRemoveAsync(user.Id, cp.Id);

            removedAgain.Should().BeFalse();
        }

        [Test]
        public async Task IsLinkedAsync_WhenLinkDoesNotExist_ReturnsFalse_ForBothIncludeSoftDeletedFlags()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            (await copyrightRepo.IsLinkedAsync("none", 123, includeSoftDeleted: false)).Should().BeFalse();
            (await copyrightRepo.IsLinkedAsync("none", 123, includeSoftDeleted: true)).Should().BeFalse();
        }

        [Test]
        public async Task QueryUserCollection_And_QueryUserLinks_ReturnEmpty_ForUserWithNoLinks()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            (await copyrightRepo.QueryUserCollection("emptyUserId").ToListAsync()).Should().BeEmpty();
            (await copyrightRepo.QueryUserLinks("emptyUserId").ToListAsync()).Should().BeEmpty();
        }

        [Test]
        public async Task QueryUserCollectionAndLinks_AreIsolatedPerUser()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = new ApplicationUser { Id = "user1Id", UserName = "firstUser", Email = "userA@test.com" };
            var user2 = new ApplicationUser { Id = "user2Id", UserName = "secondUser", Email = "userB@test.com" };

            var copyrightEntity1 = InMemoryDbContextFactory.CreateCopyright("TX-A", "firstCopyright");
            var copyrightEntity2 = InMemoryDbContextFactory.CreateCopyright("TX-B", "secondCopyright");

            testDbContext.Users.AddRange(user1, user2);
            testDbContext.CopyrightRegistrations.AddRange(copyrightEntity1, copyrightEntity2);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(user1.Id, copyrightEntity1.Id);
            await copyrightRepo.AddOrUndeleteAsync(user2.Id, copyrightEntity2.Id);

            var user1Copyrights = await copyrightRepo.QueryUserCollection(user1.Id).Select(x => x.Title).ToListAsync();
            var user2Copyrights = await copyrightRepo.QueryUserCollection(user2.Id).Select(x => x.Title).ToListAsync();

            user1Copyrights.Should().ContainSingle("firstCopyright").And.NotContain("secondCopyright");
            user2Copyrights.Should().ContainSingle("secondCopyright").And.NotContain("firstCopyright");
        }
    }
}
