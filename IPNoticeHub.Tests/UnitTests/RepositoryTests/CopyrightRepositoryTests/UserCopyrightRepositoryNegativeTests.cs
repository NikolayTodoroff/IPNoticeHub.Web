using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    public class UserCopyrightRepositoryNegativeTests : UserCopyrightRepositoryBase
    {
        [Test]
        public async Task AddOrUndeleteAsync_WhenAlreadyActive_DoesNotDuplicateRow()
        {
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-9-999-999",
                title: "Space Science",
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await repository.AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            await repository.AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            var userCopyright = 
                await testDbContext.UserCopyrights.Where(
                x => x.ApplicationUserId == user.Id && 
                x.CopyrightEntityId == entity.Id).
                ToListAsync();

            userCopyright.Should().HaveCount(1);
            userCopyright[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenMissingLink_ReturnsFalse()
        {
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-9-999-999",
                title: "Space Science",
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository repository = 
                new UserCopyrightRepository(testDbContext);

            var removed = await repository.SoftRemoveAsync(
                user.Id, 
                entity.Id);

            removed.Should().BeFalse();

            (await testDbContext.UserCopyrights.Where(
                x => x.ApplicationUserId == user.Id && 
                x.CopyrightEntityId == entity.Id).
                ToListAsync()).Should().BeEmpty();
        }

        [Test]
        public async Task SoftRemoveAsync_WhenAlreadySoftDeleted_ReturnsFalse()
        {
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-9-999-999",
                title: "Space Science",
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository repository = 
                new UserCopyrightRepository(testDbContext);

            await repository.AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            await repository.SoftRemoveAsync(
                user.Id, 
                entity.Id);

            var removedAgain = await repository.SoftRemoveAsync(
                user.Id, 
                entity.Id);

            removedAgain.Should().BeFalse();
        }

        [Test]
        public async Task IsLinkedAsync_WhenLinkDoesNotExist_ReturnsFalse_ForBothIncludeSoftDeletedFlags()
        {
            var isLinkedWithoutDeleted = await repository.IsLinkedAsync(
                "none", 
                123, 
                includeSoftDeleted: false);
            isLinkedWithoutDeleted.Should().BeFalse();

            var isLinkedWithDeleted = await repository.IsLinkedAsync(
                "none", 
                123, 
                includeSoftDeleted: true);
            isLinkedWithDeleted.Should().BeFalse();
        }

        [Test]
        public async Task QueryUserCollection_And_QueryUserLinks_ReturnEmpty_ForUserWithNoLinks()
        {
            var pagedResult = 
                await repository.GetUserCollectionPageAsync(
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
            var user2 = new ApplicationUser { 
                Id = "user2Id", 
                UserName = "TestUser2", 
                Email = "user2@test.com" };

            var entity1 =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-1-222-333",
                title: "First Copyright",
                typeOfWork: "Software",
                owner: "David Gordon",
                yearOfCreation: 2021,
                dateOfPublication: new DateTime(2021, 1, 1),
                nationOfFirstPublication: "USA");

            var entity2 =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-9-999-999",
                title: "Second Copyright",
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            testDbContext.Users.Add(user2);
            
            testDbContext.CopyrightRegistrations.AddRange(
                entity1,
                entity2);
            
            await testDbContext.SaveChangesAsync();

            await repository.AddOrUndeleteAsync(
                user.Id,
                entity1.Id); 

            await repository.AddOrUndeleteAsync(
                user2.Id,
                entity2.Id);

            var user1PagedResult = 
                await repository.GetUserCollectionPageAsync(
                userId: user.Id,
                sortBy: CollectionSortBy.DateAddedAsc,
                page: 1,
                resultsPerPage: 20,
                cancellationToken: CancellationToken.None);

            var user2PagedResult = 
                await repository.GetUserCollectionPageAsync(
                userId: user2.Id,
                sortBy: CollectionSortBy.DateAddedAsc,
                page: 1,
                resultsPerPage: 20,
                cancellationToken: CancellationToken.None);

            user1PagedResult.ResultsCount.Should().Be(1);
            user2PagedResult.ResultsCount.Should().Be(1);

            user1PagedResult.Results.Should().ContainSingle().
                Which.CopyrightEntity.Title.Should().Contain("First");

            user1PagedResult.Results.Select(x => x.CopyrightEntity.Title).
                Should().NotContain("Second");

            user2PagedResult.Results.Should().ContainSingle().
                Which.CopyrightEntity.Title.Should().Contain("Second");

            user2PagedResult.Results.Select(x => x.CopyrightEntity.Title).
                Should().NotContain("First");
        }
    }
}
