using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    [TestFixture]
    public class CopyrightRepoEdgeCaseTests
    {
        [Test]
        public async Task GetByPublicIdAsync_WithUnknownId_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var fetchedEntity = 
                await copyrightRepo.GetByPublicIdAsync(
                    Guid.NewGuid(), 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().
                BeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var fetchedEntity = 
                await copyrightRepo.GetByRegNumberAsync(
                    "TX-000000", 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().BeNull();
        }

        [Test]
        public async Task ExistsByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsFalse()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            bool regEntityExists = 
                await copyrightRepo.ExistsByRegNumberAsync(
                    "TX-000000", 
                    CancellationToken.None);

            regEntityExists.Should().
                BeFalse();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenTrimmedInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-777777", 
                    title: "TrimCheck");

            await copyrightRepo.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await copyrightRepo.GetByRegNumberAsync
                ("  TX-777777  ", 
                cancellationToken: CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenLowercaseInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-888888", 
                title: "LowercaseCheck");

            await copyrightRepo.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await copyrightRepo.GetByRegNumberAsync(
                    "tx-888888", 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().
                NotBeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenNullInput_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-123456", 
                title: "NullCheck");

            await copyrightRepo.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await copyrightRepo.GetByRegNumberAsync(
                    null!, 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().
                BeNull();
        }
    }
}
