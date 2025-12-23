using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests
{
    public class EditCopyrightServiceTests : CopyrightServiceBase
    {
        [Test]
        public async Task EditAsync_WhenLinked_UpdatesFields_AndReturnsTrue()
        {
            await SetUp();

            var initialEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(initialEntity);
            await testDbContext.SaveChangesAsync();

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                initialEntity.Id);

            var dto = new CopyrightEditDto
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

            bool result = await service.EditAsync(
                user.Id, 
                initialEntity.PublicId, 
                dto, 
                CancellationToken.None);

            result.Should().BeTrue();

            var updatedEntity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    x => x.Id == initialEntity.Id);

            updatedEntity.RegistrationNumber.Should().Be("TX-ED1-UPDATED");
            updatedEntity.TypeOfWork.Should().Be("AI-Generated Visual");
            updatedEntity.Title.Should().Be("Edited Title");
            updatedEntity.YearOfCreation.Should().Be(2024);

            updatedEntity.DateOfPublication.Should().
                Be(new DateTime(2024, 5, 6));

            updatedEntity.Owner.Should().Be("Updated Owner");
            updatedEntity.NationOfFirstPublication.Should().Be("UK");
        }

        [Test]
        public async Task EditAsync_WhenPublicIdMissing_ReturnsFalse()
        {
            await SetUp();

            var dto = new CopyrightEditDto
            {
                RegistrationNumber = "X",
                WorkType = CopyrightWorkType.Literary,
                Title = "X",
                Owner = "X"
            };

            bool result = await service.EditAsync(
                user.Id, 
                Guid.NewGuid(), 
                dto, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Test]
        public async Task EditAsync_WhenUserAndCopyrightEntityNotLinked_ReturnsFalse()
        {
            await SetUp();

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED3",
                WorkType = CopyrightWorkType.VisualArts,
                Title = "Edit Attempt",
                Owner = "Edit Attempt Owner"
            };

            bool result = await service.EditAsync(
                user.Id, 
                copyrightEntity.PublicId, 
                dto, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Test]
        public async Task EditAsync_WhenRegNumberCollides_ReturnsFalse_AndDoesNotChangeEntity()
        {
            await SetUp();

            var targetEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Target Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Target Entity Owner",
                yearOfCreation: 2021,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            var randomEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED2",
                title: "Random Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Random Entity Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.AddRange(
                targetEntity, 
                randomEntity);

            await testDbContext.SaveChangesAsync();

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                targetEntity.Id);

            var result = await service.EditAsync(
                    user.Id, 
                    targetEntity.PublicId, 
                    new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED2",
                WorkType = CopyrightWorkType.Literary,
                Title = "Replace Target Entity Title (won't be applied)",
                Owner = "New Owner (won't be applied)"
            }, CancellationToken.None);

            result.Should().BeFalse();

            var entity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    x => x.Id == targetEntity.Id);

            entity.RegistrationNumber.Should().Be("TX-ED1");
            entity.Title.Should().Be("Target Entity");
        }

        [Test]
        public async Task EditAsync_WhenRegNumberUnchanged_UpdatesOtherFields()
        {
            await SetUp();

            var entity = 
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-ED1",
                title: "Initial Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Initial Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(entity);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            await testDbContext.SaveChangesAsync();

            var dto = new CopyrightEditDto
            {
                RegistrationNumber = "TX-ED1",
                WorkType = CopyrightWorkType.PerformingArts,
                Title = "Edited Title",
                Owner = "Edited Owner"
            };

            bool result = await service.EditAsync(
                user.Id, 
                entity.PublicId, 
                dto, CancellationToken.None);

            result.Should().BeTrue();

            var editedEntity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    x => x.Id == entity.Id);

            editedEntity.RegistrationNumber.Should().Be("TX-ED1");
            editedEntity.TypeOfWork.Should().Be("PerformingArts");
            editedEntity.Title.Should().Be("Edited Title");
            editedEntity.Owner.Should().Be("Edited Owner");
        }
    }
}
