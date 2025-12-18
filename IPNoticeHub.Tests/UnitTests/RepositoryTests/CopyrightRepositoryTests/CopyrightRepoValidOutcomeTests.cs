using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Copyrights
{
    [TestFixture]
    public class CopyrightRepoValidOutcomeTests
    {
        [Test]
        public async Task Add_Then_GetByPublicId_ReturnsSameEntity()
        {
            using IPNoticeHubDbContext testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-9-999-999",
                title: "Space Science",
                owner: "David Batty",
                typeOfWork: "Software",
                yearOfCreation: 2024);

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var fetchedEntity = 
                await copyrightRepo.GetByPublicIdAsync(
                copyrightEntity.PublicId, 
                default);

            fetchedEntity.Should().
                NotBeNull();

            fetchedEntity!.Id.Should().
                Be(copyrightEntity.Id);

            fetchedEntity.PublicId.Should().
                Be(copyrightEntity.PublicId);

            fetchedEntity.Title.Should().
                Be("Space Science");
        }

        [Test]
        public async Task AddAsync_Then_GetByRegistrationNumberAsync_ReturnsSameEntity()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-123456",
                title: "Test Copyright",
                owner: "Captain America");

            await copyrightRepo.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await copyrightRepo.GetByRegNumberAsync(
                    "TX-123456",true, 
                    CancellationToken.None);

            fetchedEntity.Should().
                NotBeNull();

            fetchedEntity!.RegistrationNumber.Should().
                Be("TX-123456");

            fetchedEntity.Title.Should().
                Be("Test Copyright");
        }

        [Test]
        public async Task ExistsByRegNumberAsync_ReturnsTrueOnlyForExisting()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-654321",
                title: "Another Copyright");

            await copyrightRepo.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            bool validRegExists = 
                await copyrightRepo.ExistsByRegNumberAsync(
                    "TX-654321", 
                    CancellationToken.None);

            bool randomRegExists = 
                await copyrightRepo.ExistsByRegNumberAsync(
                    "TX-000000", 
                    CancellationToken.None);

            validRegExists.Should().
                BeTrue();

            randomRegExists.Should().
                BeFalse();
        }
    }
}
