using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Application.Copyrights.DTOs;
using IPNoticeHub.Application.Copyrights.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests
{
    [TestFixture]
    public class CopyrightServiceEditTests
    {
        [Test]
        public async Task EditAsync_WhenLinked_UpdatesFields_AndReturnsTrue()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u111", UserName = "user1", Email = "newUser@test.com" };
            testDbContext.Users.Add(user);

            var initialEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(initialEntity);
            await testDbContext.SaveChangesAsync();

            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            await userCopyrightRepo.AddOrUndeleteAsync(user.Id, initialEntity.Id);

            var copyrightRepo = new CopyrightRepository(testDbContext);
            var service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var editedEntity = new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED1-UPDATED",
                WorkType = CopyrightWorkType.Other,
                OtherWorkType = "AI-Generated Visual",
                Title = "Edited Title",
                YearOfCreation = 2024,
                DateOfPublication = new DateTime(2024, 5, 6),
                Owner = "Updated Owner",
                NationOfFirstPublication = "UK"
            };

            bool entityUpdatedSuccessfully = await service.EditAsync(user.Id, initialEntity.PublicId, editedEntity, CancellationToken.None);
            entityUpdatedSuccessfully.Should().BeTrue();

            var updatedEntity = await testDbContext.CopyrightRegistrations.SingleAsync(x => x.Id == initialEntity.Id);
            updatedEntity.RegistrationNumber.Should().Be("TX-ED1-UPDATED");
            updatedEntity.TypeOfWork.Should().Be("AI-Generated Visual");
            updatedEntity.Title.Should().Be("Edited Title");
            updatedEntity.YearOfCreation.Should().Be(2024);
            updatedEntity.DateOfPublication.Should().Be(new DateTime(2024, 5, 6));
            updatedEntity.Owner.Should().Be("Updated Owner");
            updatedEntity.NationOfFirstPublication.Should().Be("UK");
        }

        [Test]
        public async Task EditAsync_WhenPublicIdMissing_ReturnsFalse()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u111", UserName = "user1", Email = "newUser@test.com" };
            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            var copyrightEditDTO = new CopyrightEditDto
            {
                RegistrationNumber = "X",
                WorkType = CopyrightWorkType.Literary,
                Title = "X",
                Owner = "X"
            };

            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            var copyrightRepo = new CopyrightRepository(testDbContext);
            var service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            bool entityUpdatedSuccessfully = await service.EditAsync(user.Id, Guid.NewGuid(), copyrightEditDTO, CancellationToken.None);
            entityUpdatedSuccessfully.Should().BeFalse();
        }

        [Test]
        public async Task EditAsync_WhenUserAndCopyrightEntityNotLinked_ReturnsFalse()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u111", UserName = "user1", Email = "newUser@test.com" };
            testDbContext.Users.Add(user);

            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            var copyrightRepo = new CopyrightRepository(testDbContext);
            var service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var copyrightEditDTO = new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED3",
                WorkType = CopyrightWorkType.VisualArts,
                Title = "Edit Attempt",
                Owner = "Edit Attempt Owner"
            };

            bool entityUpdatedSuccessfully = await service.EditAsync(user.Id, copyrightEntity.PublicId, copyrightEditDTO, CancellationToken.None);
            entityUpdatedSuccessfully.Should().BeFalse();
        }

        [Test]
        public async Task EditAsync_WhenRegNumberCollides_ReturnsFalse_AndDoesNotChangeEntity()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u111", UserName = "user1", Email = "newUser@test.com" };
            testDbContext.Users.Add(user);

            var targetEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Target Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Target Entity Owner",
                yearOfCreation: 2021,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            var randomEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED2",
                title: "Random Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Random Entity Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.AddRange(targetEntity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            await userCopyrightRepo.AddOrUndeleteAsync(user.Id, targetEntity.Id);

            var copyrightRepo = new CopyrightRepository(testDbContext);
            var service = new CopyrightService(copyrightRepo, userCopyrightRepo);


            var entityUpdatedSuccessfully = await service.EditAsync(user.Id, targetEntity.PublicId, new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED2", // collission with "Random Entity"
                WorkType = CopyrightWorkType.Literary,
                Title = "Replace Target Entity Title (won't be applied)",
                Owner = "New Owner (won't be applied)"
            }, CancellationToken.None);

            entityUpdatedSuccessfully.Should().BeFalse();

            var targetEntityAfterEditAttempt = await testDbContext.CopyrightRegistrations.SingleAsync(x => x.Id == targetEntity.Id);
            targetEntityAfterEditAttempt.RegistrationNumber.Should().Be("TX-ED1");
            targetEntityAfterEditAttempt.Title.Should().Be("Target Entity");
        }

        [Test]
        public async Task EditAsync_WhenRegNumberUnchanged_UpdatesOtherFields()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u111", UserName = "user1", Email = "newUser@test.com" };
            testDbContext.Users.Add(user);

            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await userCopyrightRepo.AddOrUndeleteAsync(user.Id, copyrightEntity.Id);
            await testDbContext.SaveChangesAsync();
           
            var copyrightRepo = new CopyrightRepository(testDbContext);
            var service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var copyrightEditDTO = new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED1",
                WorkType = CopyrightWorkType.PerformingArts,
                Title = "Edited Title",
                Owner = "Edited Owner"
            };

            bool entityUpdatedSuccessfully = await service.EditAsync(user.Id, copyrightEntity.PublicId, copyrightEditDTO, CancellationToken.None);
            entityUpdatedSuccessfully.Should().BeTrue();

            var editedEntity = await testDbContext.CopyrightRegistrations.SingleAsync(x => x.Id == copyrightEntity.Id);
            editedEntity.RegistrationNumber.Should().Be("TX-ED1");
            editedEntity.TypeOfWork.Should().Be("PerformingArts");
            editedEntity.Title.Should().Be("Edited Title");
            editedEntity.Owner.Should().Be("Edited Owner");
        }
    }
}
