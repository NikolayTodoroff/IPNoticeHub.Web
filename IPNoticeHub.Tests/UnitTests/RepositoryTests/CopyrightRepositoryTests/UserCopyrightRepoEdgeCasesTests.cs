using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    [TestFixture]
    public class UserCopyrightRepoEdgeCasesTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_WhenAlreadyActive_DoesNotDuplicateRow()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "user7", 
                UserName = "user7", 
                Email = "u7@test" };

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-7", 
                    "copyrightRegG");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var links = await testDbContext.UserCopyrights.Where(
                x => x.ApplicationUserId == user.Id && 
                x.CopyrightEntityId == copyrightEntity.Id).
                ToListAsync();

            links.Should().
                HaveCount(1);

            links[0].IsDeleted.Should().
                BeFalse();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenMissingLink_ReturnsFalse()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u8", 
                UserName = "user8", 
                Email = "u8@test" };

            var cp = InMemoryDbContextFactory.CreateCopyright(
                "TX-8", 
                "H");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(cp);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            var removed = await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                cp.Id);

            removed.Should().
                BeFalse();

            (await testDbContext.UserCopyrights.Where(
                x => x.ApplicationUserId == user.Id && 
                x.CopyrightEntityId == cp.Id).
                ToListAsync()).Should().
                BeEmpty();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenAlreadySoftDeleted_ReturnsFalse()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u9", 
                UserName = "user9", 
                Email = "u9@test" };

            var cp = InMemoryDbContextFactory.CreateCopyright(
                "TX-9", 
                "I");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(cp);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                cp.Id);

            await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                cp.Id);

            var removedAgain = await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                cp.Id);

            removedAgain.Should().
                BeFalse();
        }

        [Test]
        public async Task IsLinkedAsync_WhenLinkDoesNotExist_ReturnsFalse_ForBothIncludeSoftDeletedFlags()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            (await copyrightRepo.IsLinkedAsync(
                "none", 
                123, 
                includeSoftDeleted: false)).Should().
                BeFalse();

            (await copyrightRepo.IsLinkedAsync(
                "none", 
                123, 
                includeSoftDeleted: true)).Should().
                BeFalse();
        }

        [Test]
        public async Task QueryUserCollection_And_QueryUserLinks_ReturnEmpty_ForUserWithNoLinks()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            var pagedResult = await copyrightRepo.GetUserCollectionPageAsync(
                userId: "emptyUserId",
                sortBy: CollectionSortBy.DateAddedAsc,
                page: 1,
                resultsPerPage: 20,
                cancellationToken: CancellationToken.None);

            pagedResult.Results.Should().BeEmpty();
            pagedResult.ResultsCount.Should().Be(0);
        }

        [Test]
        public async Task GetUserCollectionPageAsync_ReturnsOnlyItemsForGivenUser()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = new ApplicationUser { 
                Id = "user1Id", 
                UserName = "firstUser", 
                Email = "userA@test.com" };
            
            var user2 = new ApplicationUser { 
                Id = "user2Id", 
                UserName = "secondUser", 
                Email = "userB@test.com" };

            var copyrightEntity1 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-A", 
                    "firstCopyright");
            
            var copyrightEntity2 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-B", 
                    "secondCopyright");

            testDbContext.Users.AddRange(user1, user2);
            
            testDbContext.CopyrightRegistrations.AddRange(
                copyrightEntity1, 
                copyrightEntity2);
            
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(user1.Id, copyrightEntity1.Id); 
            await copyrightRepo.AddOrUndeleteAsync(user2.Id, copyrightEntity2.Id);

            var user1PagedResult = await copyrightRepo.GetUserCollectionPageAsync(
                userId: user1.Id,
                sortBy: CollectionSortBy.DateAddedAsc,
                page: 1,
                resultsPerPage: 20,
                cancellationToken: CancellationToken.None);

            var user2PagedResult = await copyrightRepo.GetUserCollectionPageAsync(
                userId: user2.Id,
                sortBy: CollectionSortBy.DateAddedAsc,
                page: 1,
                resultsPerPage: 20,
                cancellationToken: CancellationToken.None);

            var user1Items = user1PagedResult.Results;
            var user1Count = user1PagedResult.ResultsCount;

            var user2Items = user2PagedResult.Results;
            var user2Count = user2PagedResult.ResultsCount;

            var user1Titles = user1Items.
                Select(x => x.CopyrightEntity.Title).
                ToList();

            var user2Titles = user2Items.
                Select(x => x.CopyrightEntity.Title).
                ToList();

            user1Count.Should().Be(1);
            user2Count.Should().Be(1);

            user1Titles.Should()
                .ContainSingle("firstCopyright")
                .And.NotContain("secondCopyright");

            user2Titles.Should()
                .ContainSingle("secondCopyright")
                .And.NotContain("firstCopyright");
        }
    }
}
