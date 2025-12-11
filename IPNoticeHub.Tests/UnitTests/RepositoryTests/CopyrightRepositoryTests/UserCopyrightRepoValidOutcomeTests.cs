using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Copyrights
{
    [TestFixture]
    public class UserCopyrightRepoValidOutcomeTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_NewLink_CreatesActiveRow()
        {
            using IPNoticeHubDbContext testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u1", 
                UserName = "user1", 
                Email = "u1@test.com" };

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-1234567", 
                    title: "Test Copyright Registration");
            
            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id, 
                CancellationToken.None);

            var link = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == copyrightEntity.Id);

            link.IsDeleted.Should().
                BeFalse();
        }

        [Test]
        public async Task AddOrUndeleteAsync_WhenSoftDeleted_UndeletesAndUpdatesDate()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u2", 
                UserName = "user2", 
                Email = "user2@test" };

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-2123455", 
                    title: "B Reg");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var initialLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == copyrightEntity.Id);


            await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                copyrightEntity.Id);

            var softDeletedLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == copyrightEntity.Id);

            softDeletedLink.IsDeleted.Should().
                BeTrue();

            var initialDate = softDeletedLink.DateAdded;
            await Task.Delay(5);
            await copyrightRepo.AddOrUndeleteAsync(user.Id, copyrightEntity.Id);

            var undeletedLink = 
                await testDbContext.UserCopyrights.SingleAsync(
                    x => x.ApplicationUserId == user.Id && 
                    x.CopyrightEntityId == copyrightEntity.Id);

            undeletedLink.IsDeleted.Should().
                BeFalse();

            undeletedLink.DateAdded.Should().
                BeOnOrAfter(initialDate);
        }

        [Test]
        public async Task IsLinkedAsync_RespectsIncludeSoftDeletedFlag()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u3", 
                UserName = "user3", 
                Email = "u3@test" };

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-3", 
                    title: "C");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            (await copyrightRepo.IsLinkedAsync(
                user.Id, 
                copyrightEntity.Id, 
                includeSoftDeleted: false)).Should().
                BeTrue();
           
            (await copyrightRepo.IsLinkedAsync(
                user.Id, 
                copyrightEntity.Id, 
                includeSoftDeleted: true)).Should().
                BeTrue();

            await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                copyrightEntity.Id);

            (await copyrightRepo.IsLinkedAsync(
                user.Id, 
                copyrightEntity.Id, 
                includeSoftDeleted: false)).Should().
                BeFalse();
           
            (await copyrightRepo.IsLinkedAsync(
                user.Id, 
                copyrightEntity.Id, 
                includeSoftDeleted: true)).Should().
                BeTrue();
        }

        [Test]
        public async Task QueryUserCollection_ReturnsOnlyActiveEntities()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u4", 
                UserName = "user4", 
                Email = "u4@test" };

            var copyrightEntity1 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-4A", 
                    "copyrightRegA");
            
            var copyrightEntity2 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-4B", 
                    "copyrightRegB");

            testDbContext.Users.Add(user);

            testDbContext.CopyrightRegistrations.AddRange(
                copyrightEntity1, 
                copyrightEntity2);
           
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity1.Id);
            
            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity2.Id);
            
            await copyrightRepo.SoftRemoveAsync(
                user.Id, 
                copyrightEntity2.Id);

            var validEntities = 
                await copyrightRepo.QueryUserCollection(user.Id).
                Select(x => x.Title).
                ToListAsync();

            validEntities.Should().
                ContainSingle("copyrightRegA");
            
            validEntities.Should().
                NotContain("copyrightRegB");
        }

        [Test]
        public async Task QueryUserLinks_ReturnsActiveLinksWithIncludedRegistration()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "user5", 
                UserName = "user5", 
                Email = "u5@test" };
            
            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-54321", 
                    "copyrightRegE");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var links = await copyrightRepo.
                QueryUserLinks(user.Id).
                ToListAsync();

            links.Should().
                HaveCount(1);
            
            links[0].IsDeleted.Should().
                BeFalse();
            
            links[0].CopyrightEntity.Should().
                NotBeNull();
            links[0].CopyrightEntity!.Title.Should().
                Be("copyrightRegE");
        }

        [Test]
        public async Task SoftRemoveAsync_ActiveLink_SoftDeletesAndReturnsTrue()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u6", 
                UserName = "user6", 
                Email = "u6@test" };

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-654321", 
                    "copyrightRegF");

            testDbContext.Users.Add(user);
            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            IUserCopyrightRepository copyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await copyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var removedSuccessfully = 
                await copyrightRepo.SoftRemoveAsync(
                    user.Id, 
                    copyrightEntity.Id);

            removedSuccessfully.Should().
                BeTrue();

            (await testDbContext.UserCopyrights.SingleAsync(
                x => x.ApplicationUserId == user.Id && 
                x.CopyrightEntityId == copyrightEntity.Id)).IsDeleted.Should().
                BeTrue();
        }
    }
}
