using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Copyrights
{
    public class CopyrightServiceTests : CopyrightServiceBase
    {
        [Test]
        public async Task CreateAsync_WithNewRegistration_CreatesEntity_AssociatesUser_AndReturnsPublicId()
        {
            await SetUp();

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = "TX-111111",
                WorkType = CopyrightWorkType.Literary,
                Title = "Just Created",
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
                    c => c.RegistrationNumber == "TX-111111");

            entity.PublicId.Should().Be(publicId);
            entity.Title.Should().Be("Just Created");

            var link = 
                await testDbContext.UserCopyrights.SingleAsync(
                    l => l.ApplicationUserId == user.Id && 
                    l.CopyrightEntityId == entity.Id);

            link.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WhenWorkTypeOther_StoresCustomString_AndLinksUser()
        {
            await SetUp();

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

            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WithExistingRegistration_ReusesEntity_AndLinksUserWithoutDuplication()
        {
            await SetUp();

            var existingCopyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-333333", 
                    title: "ActionMan");

            testDbContext.CopyrightRegistrations.Add(existingCopyrightEntity);

            await testDbContext.SaveChangesAsync();

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = "TX-333333",
                WorkType = CopyrightWorkType.Audiovisual,
                Title = "ActionMan (ignored)",
                Owner = "The Butcher"
            };

            var publicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            (await testDbContext.CopyrightRegistrations.CountAsync(
                c => c.RegistrationNumber == "TX-333333")).
                Should().Be(1);

            publicId.Should().Be(existingCopyrightEntity.PublicId);

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == existingCopyrightEntity.Id);

            userCopyright.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task GetDetailsAsync_WhenLinked_ReturnsDetailsDTO_WithStoredTypeOfWorkString()
        {
            await SetUp();

            var entity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-444444",
                title: "Delta Force",
                typeOfWork: CopyrightWorkType.VisualArts.ToString(),
                owner: "Owner D");

            testDbContext.CopyrightRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).
                AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            var dto = 
                await service.GetDetailsAsync(
                    user.Id, 
                    entity.PublicId, 
                    CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Title.Should().Be("Delta Force");
            dto.TypeOfWork.Should().Be("VisualArts");
        }

        [Test]
        public async Task RemoveAsync_WhenLinked_ReturnsTrue_AndSoftDeletes()
        {
            await SetUp();

            var entity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-555555", 
                    title: "Titanic 1000");

            testDbContext.CopyrightRegistrations.Add(entity);

            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).AddOrUndeleteAsync(
                user.Id, 
                entity.Id);

            var isLinkSoftRemoved = await service.RemoveAsync(
                user.Id, 
                entity.PublicId, 
                CancellationToken.None);

            isLinkSoftRemoved.Should().BeTrue();

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == entity.Id);

            userCopyright.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenPageOrSizeInvalid_NormalizesAndReturnsData()
        {
            await SetUp();

            var entity1 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-N1", 
                    "Alpha");
            
            var entity2 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-N2", 
                    "Beta");

            testDbContext.CopyrightRegistrations.AddRange(
                entity1, 
                entity2);

            await testDbContext.SaveChangesAsync();

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                entity1.Id);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                entity2.Id);

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id,
                sortBy: CollectionSortBy.DateAddedDesc,
                page: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(2);
            pagedResult.CurrentPage.Should().BeGreaterThan(0);
            pagedResult.ResultsCountPerPage.Should().BeGreaterThan(0);
            pagedResult.Results.Should().NotBeEmpty();
        }
    }
}
