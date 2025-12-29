using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests.CopyrightServiceTests
{
    public class CopyrightServiceTests : CopyrightServiceBase
    {
        [Test]
        public async Task CreateAsync_WithNewRegistration_CreatesEntity_AssociatesUser_AndReturnsPublicId()
        {
            const string expectedRegistrationNumber = "TX-111111";
            const string expectedTitle = "Just Created";
            const string ownerName = "Test Owner";
            const string nationOfPublication = "Canada";
            
            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = expectedRegistrationNumber,
                WorkType = CopyrightWorkType.Literary,
                Title = expectedTitle,
                YearOfCreation = 2024,
                DateOfPublication = new DateTime(2024, 5, 1),
                Owner = ownerName,
                NationOfFirstPublication = nationOfPublication
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
            const string expectedRegNumber = "TX-111111";
            const string expectedTitle = "Test Title";
            const string expectedOtherWorkType = "Test Type";
            const string expectedOwner = "Test Owner";

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = expectedRegNumber,
                WorkType = CopyrightWorkType.Other,
                OtherWorkType = expectedOtherWorkType,
                Title = expectedTitle,
                Owner = expectedOwner
            };

            Guid publicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            var entity = 
                await testDbContext.CopyrightRegistrations.SingleAsync(
                    c => c.RegistrationNumber == expectedRegNumber);

            entity.PublicId.Should().Be(publicId);
            entity.TypeOfWork.Should().Be(expectedOtherWorkType);
            entity.Title.Should().Be(expectedTitle);

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
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-123456",
                title: "Test Copyright Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Owner",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2020, 1, 2),
                nationOfFirstPublication: "US");

            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = entity.RegistrationNumber,
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
                    c => c.RegistrationNumber == entity.RegistrationNumber);

            registrationCount.Should().Be(1);

            publicId.Should().Be(entity.PublicId);

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == entity.Id);

            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task GetDetailsAsync_WhenLinked_ReturnsDetailsDTO_WithStoredTypeOfWorkString()
        {
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-654321",
                title: "Test Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Copyright Owner",
                yearOfCreation: 2022,
                dateOfPublication: new DateTime(2023, 2, 5),
                nationOfFirstPublication: "Canada");

            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id,
                entity.Id);

            var dto = 
                await service.GetDetailsAsync(
                    user.Id,
                    entity.PublicId, 
                    CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Title.Should().Be(entity.Title);
            dto.TypeOfWork.Should().Be(entity.TypeOfWork);
        }

        [Test]
        public async Task RemoveAsync_WhenLinked_ReturnsTrue_AndSoftDeletes()
        {
            var entity =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-654321",
                title: "Test Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Copyright Owner",
                yearOfCreation: 2022,
                dateOfPublication: new DateTime(2025, 5, 5),
                nationOfFirstPublication: "Germany");

            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id,
                entity.Id);

            var isSoftRemoved = await service.RemoveAsync(
                user.Id,
                entity.PublicId, 
                CancellationToken.None);

            isSoftRemoved.Should().BeTrue();

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == entity.Id);

            userCopyright.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenPageOrSizeInvalid_NormalizesAndReturnsData()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-654321",
                title: "Test Entity",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Copyright Owner",
                yearOfCreation: 2022,
                dateOfPublication: new DateTime(2025, 5, 5),
                nationOfFirstPublication: "Germany");

            var entity2 =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-1122334",
                title: "Test Entity 2",
                typeOfWork: CopyrightWorkType.Literary.ToString(),
                owner: "Test Copyright Owner 2",
                yearOfCreation: 2023,
                dateOfPublication: new DateTime(2022, 1, 1),
                nationOfFirstPublication: "USA");

            var entity3 =
                InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-442211",
                title: "Test Entity 3",
                typeOfWork: CopyrightWorkType.VisualArts.ToString(),
                owner: "Test Copyright Owner 3",
                yearOfCreation: 2020,
                dateOfPublication: new DateTime(2021, 11, 15),
                nationOfFirstPublication: "France");

            testDbContext.CopyrightRegistrations.Add(entity1);
            testDbContext.CopyrightRegistrations.Add(entity2);
            testDbContext.CopyrightRegistrations.Add(entity3);
            await testDbContext.SaveChangesAsync();

            await Task.WhenAll(
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, entity1.Id),
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, entity2.Id),
                userCopyrightRepo.AddOrUndeleteAsync(user.Id, entity3.Id));

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
