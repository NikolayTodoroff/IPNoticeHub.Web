using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Copyrights
{
    public class UserCopyrightRepositoryTests : UserCopyrightRepositoryBase
    {
        [Test]
        public async Task AddOrUndeleteAsync_NewLink_CreatesActiveRow()
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
                entity.Id, 
                CancellationToken.None);

            var userCopyright = 
                await testDbContext.UserCopyrights
                    .SingleAsync(
                        x => x.ApplicationUserId == user.Id && 
                        x.CopyrightEntityId == entity.Id,
                        CancellationToken.None);

            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddOrUndeleteAsync_WhenSoftDeleted_UndeletesAndUpdatesDate()
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

            var initialLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == entity.Id);

            await repository.SoftRemoveAsync(
                user.Id,
                entity.Id,
                CancellationToken.None);

            var softDeletedLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == entity.Id,
                    CancellationToken.None);

            softDeletedLink.IsDeleted.Should().BeTrue();

            var initialDate = softDeletedLink.DateAdded;
            await Task.Delay(5, CancellationToken.None);
            await repository.AddOrUndeleteAsync(user.Id, entity.Id, CancellationToken.None);

            var undeletedLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == entity.Id,
                    CancellationToken.None);

            undeletedLink.IsDeleted.Should().BeFalse();
            undeletedLink.DateAdded.Should().BeOnOrAfter(initialDate);
        }

        [Test]
        public async Task IsLinkedAsync_RespectsIncludeSoftDeletedFlag()
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

            var isLinkedActive = 
                await repository.IsLinkedAsync(
                user.Id, 
                entity.Id, 
                includeSoftDeleted: false);
            isLinkedActive.Should().BeTrue();
           
            var isLinkedIncludingDeleted = 
                await repository.IsLinkedAsync(
                user.Id, 
                entity.Id, 
                includeSoftDeleted: true);

            isLinkedIncludingDeleted.Should().BeTrue();

            await repository.SoftRemoveAsync(
                user.Id, 
                entity.Id);

            var isLinkedAfterDelete = 
                await repository.IsLinkedAsync(
                user.Id, 
                entity.Id, 
                includeSoftDeleted: false);

            isLinkedAfterDelete.Should().BeFalse();
           
            var isLinkedIncludingDeletedAfter = 
                await repository.IsLinkedAsync(
                user.Id, 
                entity.Id, 
                includeSoftDeleted: true);

            isLinkedIncludingDeletedAfter.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionPageAsync_ReturnsActiveLinksWithIncludedRegistration()
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

            await repository.AddOrUndeleteAsync(user.Id, entity.Id, CancellationToken.None);

            var pageResult = 
                await repository.GetUserCollectionPageAsync(
                user.Id,
                CollectionSortBy.DateAddedDesc,
                page: 1,
                resultsPerPage: 10,
                cancellationToken: CancellationToken.None);

            var links = pageResult.Results;

            links.Should().HaveCount(1);
            var link = links[0];
            link.IsDeleted.Should().BeFalse();
            link.CopyrightEntity.Should().NotBeNull();
            link.CopyrightEntity!.Title.Should().Be(entity.Title);
        }

        [Test]
        public async Task SoftRemoveAsync_ActiveLink_SoftDeletesAndReturnsTrue()
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

            var removed = 
                await repository.SoftRemoveAsync(user.Id, entity.Id);

            removed.Should().BeTrue();

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == entity.Id);

            userCopyright.IsDeleted.Should().BeTrue();
        }
    }
}
