using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    [TestFixture]
    public class CopyrightRepositoryNegativeTests : CopyrightRepositoryBase
    {
        [Test]
        public async Task GetByPublicIdAsync_WithUnknownId_ReturnsNull()
        {
            var entity = 
                await repository.GetByPublicIdAsync(
                    Guid.NewGuid(), 
                    cancellationToken: CancellationToken.None);

            entity.Should().BeNull();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsNull()
        {
            const string testRegNumber = "TX-000000";

            var entity = 
                await repository.GetByRegNumberAsync(
                    testRegNumber, 
                    cancellationToken: CancellationToken.None);

            entity.Should().BeNull();
        }

        [Test]
        public async Task ExistsByRegistrationNumberAsync_WithUnknownRegNumber_ReturnsFalse()
        {
            const string testRegNumber = "TX-000000";

            bool entityExists = 
                await repository.ExistsByRegNumberAsync(
                    testRegNumber, 
                    CancellationToken.None);

            entityExists.Should().BeFalse();
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenTrimmedInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            const string registrationNumber = "TX-777777";

            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: registrationNumber,
                title: "TrimCheck",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            await repository.AddAsync(
                entity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                $"  {registrationNumber}  ", 
                cancellationToken: CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
            fetchedEntity!.RegistrationNumber.Should().Be(registrationNumber);
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenLowercaseInput_ReturnsMatch_IfRepositoryNormalizes()
        {
            const string registrationNumber = "TX-888888";
            const string searchQuery = "tx-888888";

            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: registrationNumber,
                title: "LowercaseCheck",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            await repository.AddAsync(
                entity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    searchQuery, 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
            fetchedEntity!.RegistrationNumber.Should().Be(registrationNumber);
        }

        [Test]
        public async Task GetByRegistrationNumberAsync_WhenNullInput_ReturnsNull()
        {
            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    null!, 
                    cancellationToken: CancellationToken.None);

            fetchedEntity.Should().BeNull();
        }
    }
}
