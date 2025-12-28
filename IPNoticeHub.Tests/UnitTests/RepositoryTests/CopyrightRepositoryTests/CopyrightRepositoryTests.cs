using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Copyrights
{
    public class CopyrightRepositoryTests : CopyrightRepositoryBase
    {
        [Test]
        public async Task Add_Then_GetByPublicId_ReturnsSameEntity()
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

            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            var fetchedEntity = 
                await repository.GetByPublicIdAsync(
                entity.PublicId, 
                default);

            fetchedEntity.Should().NotBeNull();
            fetchedEntity!.Id.Should().Be(entity.Id);
            fetchedEntity.PublicId.Should().Be(entity.PublicId);
            fetchedEntity.Title.Should().Be(entity.Title);
        }

        [Test]
        public async Task AddAsync_Then_GetByRegistrationNumberAsync_ReturnsSameEntity()
        {
            const string registrationNumber = "TX-123456";
            const string title = "Test Copyright";

            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: registrationNumber,
                title: title,
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            await repository.AddAsync(
                entity, 
                CancellationToken.None);

            var fetchedEntity = 
                await repository.GetByRegNumberAsync(
                    registrationNumber,true, 
                    CancellationToken.None);

            fetchedEntity.Should().NotBeNull();
            fetchedEntity!.RegistrationNumber.Should().Be(registrationNumber);
            fetchedEntity.Title.Should().Be(title);
        }

        [Test]
        public async Task ExistsByRegNumberAsync_ReturnsTrueOnlyForExisting()
        {
            const string existingRegNumber = "TX-654321";
            const string nonExistentRegNumber = "TX-000000";

            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: existingRegNumber,
                title: "Space Science",
                typeOfWork: "Software",
                owner: "David Batty",
                yearOfCreation: 2024,
                dateOfPublication: new DateTime(2020, 1, 1),
                nationOfFirstPublication: "USA");

            await repository.AddAsync(
                entity, 
                CancellationToken.None);

            bool validRegExists = 
                await repository.ExistsByRegNumberAsync(
                    existingRegNumber, 
                    CancellationToken.None);

            bool randomRegExists = 
                await repository.ExistsByRegNumberAsync(
                    nonExistentRegNumber, 
                    CancellationToken.None);

            validRegExists.Should().BeTrue();
            randomRegExists.Should().BeFalse();
        }
    }
}
