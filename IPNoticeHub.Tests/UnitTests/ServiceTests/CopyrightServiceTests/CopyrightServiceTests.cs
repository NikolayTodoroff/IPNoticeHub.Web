using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Copyrights
{
    public class CopyrightServiceTests : CopyrightServiceBase
    {
        [Test]
        public async Task CreateAsync_WithNewRegistration_CreatesEntity_AssociatesUser_AndReturnsPublicId()
        {
            const string expectedRegistrationNumber = "TX-111111";
            const string expectedTitle = "Just Created";
            
            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = expectedRegistrationNumber,
                WorkType = CopyrightWorkType.Literary,
                Title = expectedTitle,
                YearOfCreation = 2024,
                DateOfPublication = new DateTime(2024, 5, 1),
                Owner = "default Owner",
                NationOfFirstPublication = "US"
            };

            Guid publicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            var entity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == expectedRegistrationNumber);

            entity.PublicId.Should().Be(publicId);
            entity.Title.Should().Be(expectedTitle);

            var link = 
                await testDbContext.UserCopyrights.SingleAsync(
                    l => l.ApplicationUserId == user.Id && 
                    l.CopyrightEntityId == entity.Id);

            link.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WhenWorkTypeOther_StoresCustomString_AndLinksUser()
        {
            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = "TX-222222",
                WorkType = CopyrightWorkType.Other,
                OtherWorkType = "AI-Generated Visual",
                Title = "Custom Registration",
                Owner = "New Owner"
            };

            Guid publicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            var entity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == "TX-222222");

            entity.PublicId.Should().Be(publicId);
            entity.TypeOfWork.Should().Be("AI-Generated Visual");
            entity.Title.Should().Be("Custom Registration");

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == entity.Id);

            userCopyright.Should().NotBeNull();
            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WithExistingRegistration_ReusesEntity_AndLinksUserWithoutDuplication()
        {
            var existingEntity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == cpEntity1.RegistrationNumber);
            
            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = cpEntity1.RegistrationNumber,
                WorkType = CopyrightWorkType.Literary,
                Title = "Title 1",
                Owner = "Owner 1"
            };

            var publicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            var registrationCount = 
                await testDbContext.CopyrightRegistrations.CountAsync(
                    c => c.RegistrationNumber == cpEntity1.RegistrationNumber);

            registrationCount.Should().Be(1);

            publicId.Should().Be(cpEntity1.PublicId);

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == cpEntity1.Id);

            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task GetDetailsAsync_WhenLinked_ReturnsDetailsDTO_WithStoredTypeOfWorkString()
        {
            var existingEntity =
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == cpEntity1.RegistrationNumber);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id,
                existingEntity.Id);

            var dto = 
                await service.GetDetailsAsync(
                    user.Id,
                    existingEntity.PublicId, 
                    CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Title.Should().Be(existingEntity.Title);
            dto.TypeOfWork.Should().Be(existingEntity.TypeOfWork);
        }

        [Test]
        public async Task RemoveAsync_WhenLinked_ReturnsTrue_AndSoftDeletes()
        {
            var existingEntity =
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == cpEntity1.RegistrationNumber);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id,
                existingEntity.Id);

            var isSoftRemoved = await service.RemoveAsync(
                user.Id,
                existingEntity.PublicId, 
                CancellationToken.None);

            isSoftRemoved.Should().BeTrue();

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == existingEntity.Id);

            userCopyright.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenPageOrSizeInvalid_NormalizesAndReturnsData()
        {
            await Task.WhenAll(
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, cpEntity1.Id),
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, cpEntity2.Id),
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, cpEntity3.Id));

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id,
                sortBy: CollectionSortBy.DateAddedDesc,
                page: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(3);
            pagedResult.CurrentPage.Should().BeGreaterThan(0);
            pagedResult.ResultsCountPerPage.Should().BeGreaterThan(0);
            pagedResult.Results.Should().NotBeEmpty();
        }
    }
}
