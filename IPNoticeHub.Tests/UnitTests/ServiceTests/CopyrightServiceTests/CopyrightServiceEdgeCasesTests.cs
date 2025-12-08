using FluentAssertions;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Services.Copyrights.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests
{
    [TestFixture]
    public class CopyrightServiceEdgeCasesTests
    {
        [Test]
        public async Task GetDetailsAsync_WhenPublicIdDoesNotExist_ReturnsNull()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "1234567", UserName = "user1", Email = "user@test.com" };
            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = new CopyrightRepository(testDbContext);
            IUserCopyrightRepository userCopyrightRepo = new UserCopyrightRepository(testDbContext);

            var copyrightService = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var copyrightResultDTO = await copyrightService.GetDetailsAsync(user.Id, Guid.NewGuid(), CancellationToken.None);

            copyrightResultDTO.Should().BeNull();
        }

        [Test]
        public async Task GetDetailsAsync_WhenNotLinked_ReturnsNull()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "1234567", UserName = "user1", Email = "user@test.com" };
            testDbContext.Users.Add(user);

            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright("TX-E1", "Orphan");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = new CopyrightRepository(testDbContext);
            IUserCopyrightRepository userCopyrightRepo = new UserCopyrightRepository(testDbContext);

            var copyrightService = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var result = await copyrightService.GetDetailsAsync(user.Id, copyrightEntity.PublicId, CancellationToken.None);

            result.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_WhenPublicIdMissing_ReturnsFalse()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "1234567", UserName = "user1", Email = "user@test.com" };
            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = new CopyrightRepository(testDbContext);
            IUserCopyrightRepository userCopyrightRepo = new UserCopyrightRepository(testDbContext);

            var copyrightService = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var result = await copyrightService.RemoveAsync(user.Id, Guid.NewGuid(), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenEmpty_ReturnsEmptyPage()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "1234567", UserName = "user1", Email = "user@test.com" };
            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = new CopyrightRepository(testDbContext);
            IUserCopyrightRepository userCopyrightRepo = new UserCopyrightRepository(testDbContext);

            var copyrightService = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var pagedResult = await copyrightService.GetUserCollectionAsync(
                userId: user.Id,
                sortBy: Common.EnumConstants.CollectionSortBy.DateAddedDesc,
                page: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(0);
            pagedResult.Results.Should().BeEmpty();
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(10);
        }
    }
}
