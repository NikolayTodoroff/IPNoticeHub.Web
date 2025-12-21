using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Services.CopyrightService.Implementations;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Copyrights
{
    [TestFixture]
    public class CopyrightServiceValidOutcomeTests
    {
        [Test]
        public async Task CreateAsync_WithNewRegistration_CreatesEntity_AssociatesUser_AndReturnsPublicId()
        {
            using IPNoticeHubDbContext testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u1", 
                UserName = "user1", 
                Email = "user1@test.com" };

            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = 
                new CopyrightRepository(testDbContext);

            IUserCopyrightRepository userCopyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            ICopyrightService service = 
                new CopyrightService(
                copyrightRepo, 
                userCopyrightRepo);

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

            entity.PublicId.Should().
                Be(publicId);

            entity.Title.Should().
                Be("Just Created");

            var link = 
                await testDbContext.UserCopyrights.SingleAsync(
                    l => l.ApplicationUserId == user.Id && 
                    l.CopyrightEntityId == entity.Id);

            link.IsDeleted.Should().
                BeFalse();
        }

        [Test]
        public async Task CreateAsync_WhenWorkTypeOther_StoresCustomString_AndLinksUser()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u2", 
                UserName = "user2", 
                Email = "user2@test.com" };

            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            var copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var userCopyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            ICopyrightService service = 
                new CopyrightService(
                    copyrightRepo, 
                    userCopyrightRepo);

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

            entity.PublicId.Should().
                Be(publicId);

            entity.TypeOfWork.Should().
                Be("AI-Generated Visual");

            entity.Title.Should().
                Be("Custom Registration");

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == entity.Id);

            userCopyright.IsDeleted.Should().
                BeFalse();
        }

        [Test]
        public async Task CreateAsync_WithExistingRegistration_ReusesEntity_AndLinksUserWithoutDuplication()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u3", 
                UserName = "user3", 
                Email = "user3@test.com" };
            
            testDbContext.Users.Add(user);

            var existingCopyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-333333", 
                    title: "ActionMan");

            testDbContext.CopyrightRegistrations.Add(existingCopyrightEntity);

            await testDbContext.SaveChangesAsync();

            var copyrightRepo = 
                new CopyrightRepository(testDbContext);

            var userCopyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            ICopyrightService service = 
                new CopyrightService(
                    copyrightRepo, 
                    userCopyrightRepo);

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = "TX-333333",
                WorkType = CopyrightWorkType.Audiovisual,
                Title = "ActionMan (ignored)",
                Owner = "The Butcher"
            };

            var returnedEntityPublicId = await service.CreateAsync(
                user.Id, 
                dto, 
                CancellationToken.None);

            (await testDbContext.CopyrightRegistrations.CountAsync(
                c => c.RegistrationNumber == "TX-333333")).Should().
                Be(1);

            returnedEntityPublicId.Should().
                Be(existingCopyrightEntity.PublicId);

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == existingCopyrightEntity.Id);

            userCopyright.IsDeleted.Should().
                BeFalse();
        }

        [Test]
        public async Task GetDetailsAsync_WhenLinked_ReturnsDetailsDTO_WithStoredTypeOfWorkString()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u4", 
                UserName = "user4", 
                Email = "u4@test.com" };

            testDbContext.Users.Add(user);


            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-444444",
                title: "Delta Force",
                typeOfWork: CopyrightWorkType.VisualArts.ToString(),
                owner: "Owner D");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).
                AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var copyrightService = 
                new CopyrightService(
                    new CopyrightRepository(testDbContext), 
                    new UserCopyrightRepository(testDbContext));

            var dto = 
                await copyrightService.GetDetailsAsync(
                    user.Id, 
                    copyrightEntity.PublicId, 
                    CancellationToken.None);

            dto.Should().
                NotBeNull();

            dto!.Title.Should().
                Be("Delta Force");

            dto.TypeOfWork.Should().
                Be("VisualArts");
        }

        [Test]
        public async Task RemoveAsync_WhenLinked_ReturnsTrue_AndSoftDeletes()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "u5", 
                UserName = "user5", 
                Email = "u5@test" };

            testDbContext.Users.Add(user);

            var copyrightEntity = 
                InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "TX-555555", 
                    title: "Titanic 1000");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);

            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity.Id);

            var copyrightService = new CopyrightService(
                new CopyrightRepository(testDbContext), 
                new UserCopyrightRepository(testDbContext));

            var isLinkSoftRemoved = await copyrightService.RemoveAsync(
                user.Id, 
                copyrightEntity.PublicId, 
                CancellationToken.None);

            isLinkSoftRemoved.Should().
                BeTrue();

            var userCopyright = 
                await testDbContext.UserCopyrights.SingleAsync(
                    uc => uc.ApplicationUserId == user.Id && 
                    uc.CopyrightEntityId == copyrightEntity.Id);

            userCopyright.IsDeleted.Should().
                BeTrue();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenPageOrSizeInvalid_NormalizesAndReturnsData()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { 
                Id = "111", 
                UserName = "newUser", 
                Email = "defaultUser@test.com" };

            testDbContext.Users.Add(user);

            var copyrightEntity1 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-N1", 
                    "Alpha");
            
            var copyrightEntity2 = 
                InMemoryDbContextFactory.CreateCopyright(
                    "TX-N2", 
                    "Beta");

            testDbContext.CopyrightRegistrations.AddRange(
                copyrightEntity1, 
                copyrightEntity2);

            await testDbContext.SaveChangesAsync();


            var userCopyrightRepo = 
                new UserCopyrightRepository(testDbContext);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity1.Id);

            await userCopyrightRepo.AddOrUndeleteAsync(
                user.Id, 
                copyrightEntity2.Id);

            var service = 
                new CopyrightService(
                    new CopyrightRepository(testDbContext), 
                    userCopyrightRepo);

            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id,
                sortBy: CollectionSortBy.DateAddedDesc,
                page: 0,
                resultsPerPage: 0,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(2);

            pagedResult.CurrentPage.Should().
                BeGreaterThan(0);

            pagedResult.ResultsCountPerPage.Should().
                BeGreaterThan(0);

            pagedResult.Results.Should().NotBeEmpty();
        }
    }
}
