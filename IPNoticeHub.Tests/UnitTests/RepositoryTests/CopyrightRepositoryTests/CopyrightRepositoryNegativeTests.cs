using FluentAssertions;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    [TestFixture]
    public class CopyrightRepositoryNegativeTests
    {
        [Test]
        public async Task GetByPublicIdAsync_WithUnknownId_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository repository = 
                new CopyrightRepository(testDbContext);

            var fetchedEntity = 
                await repository.GetByPublicIdAsync(
                    Guid.NewGuid(), 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().BeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository repository = 
                new CopyrightRepository(testDbContext);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    "TX-000000", 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().BeNull();
        }

        [Test]
        public async Task ExistsByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsFalse()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository repository = 
                new CopyrightRepository(testDbContext);

            bool regEntityExists = 
                await repository.ExistsByRegNumberAsync(
                    "TX-000000", 
                    CancellationToken.None);

            regEntityExists.Should().BeFalse();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenTrimmedInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ICopyrightRepository repository = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-777777", 
                    title: "TrimCheck");

            await repository.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync
                ("  TX-777777  ", 
                cancellationToken: CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenLowercaseInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var repository = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-888888", 
                title: "LowercaseCheck");

            await repository.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    "tx-888888", 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenNullInput_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var repository = 
                new CopyrightRepository(testDbContext);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-123456", 
                title: "NullCheck");

            await repository.AddAsync(
                copyrightEntity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    null!, 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().BeNull();
        }
    }
}
