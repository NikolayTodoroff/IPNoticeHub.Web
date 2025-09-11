using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using IPNoticeHub.Data.Repositories.Copyrights.Implementations;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Services.Copyrights.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Copyrights
{
    /// <summary>
    /// Summary: CopyrightService – Create, Retrieve Details, and Remove Operations
    /// - Validates that CreateAsync with a new registration creates a copyright entity, associates it with the user, and returns the PublicId.
    /// - Confirms that CreateAsync with an existing registration number avoids duplication, reuses the existing entity, and links it to the user.
    /// - Ensures GetDetailsAsync for a linked copyright returns a fully populated details DTO.
    /// - Verifies that RemoveAsync for a linked and existing entity returns true and performs a soft delete of the link.
    /// </summary>
    [TestFixture]
    public class CopyrightServiceValidOutcomeTests
    {
        [Test]
        public async Task CreateAsync_WithNewRegistration_CreatesEntity_AssociatesUser_AndReturnsPublicId()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u1", UserName = "user1", Email = "user1@test.com" };
            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            ICopyrightRepository copyrightRepo = new CopyrightRepository(testDbContext);
            IUserCopyrightRepository userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            ICopyrightService service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var createCopyrightDTO = new CopyrightCreateDTO
            {
                RegistrationNumber = "TX-111111",
                WorkType = CopyrightWorkType.Literary,
                Title = "Just Created",
                YearOfCreation = 2024,
                DateOfPublication = new DateTime(2024, 5, 1),
                Owner = "default Owner",
                NationOfFirstPublication = "US"
            };

            Guid publicId = await service.CreateAsync(user.Id, createCopyrightDTO, CancellationToken.None);

            var entity = await testDbContext.CopyrightRegistrations.SingleAsync(c => c.RegistrationNumber == "TX-111111");

            entity.PublicId.Should().Be(publicId);
            entity.Title.Should().Be("Just Created");

            var link = await testDbContext.UserCopyrights.SingleAsync(l => l.ApplicationUserId == user.Id && l.CopyrightRegistrationId == entity.Id);
            link.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WhenWorkTypeOther_StoresCustomString_AndLinksUser()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u2", UserName = "user2", Email = "user2@test.com" };
            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            var copyrightRepo = new CopyrightRepository(testDbContext);
            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            ICopyrightService service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var createCopyrightDTO = new CopyrightCreateDTO
            {
                RegistrationNumber = "TX-222222",
                WorkType = CopyrightWorkType.Other,
                OtherWorkType = "AI-Generated Visual",
                Title = "Custom Registration",
                Owner = "New Owner"
            };

            Guid publicId = await service.CreateAsync(user.Id, createCopyrightDTO, CancellationToken.None);

            var entity = await testDbContext.CopyrightRegistrations.SingleAsync(c => c.RegistrationNumber == "TX-222222");

            entity.PublicId.Should().Be(publicId);
            entity.TypeOfWork.Should().Be("AI-Generated Visual");
            entity.Title.Should().Be("Custom Registration");

            var link = await testDbContext.UserCopyrights
                .SingleAsync(l => l.ApplicationUserId == user.Id && l.CopyrightRegistrationId == entity.Id);

            link.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task CreateAsync_WithExistingRegistration_ReusesEntity_AndLinksUserWithoutDuplication()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u3", UserName = "user3", Email = "user3@test.com" };
            testDbContext.Users.Add(user);

            var existingCopyrightEntity = InMemoryDbContextFactory.CreateCopyright(registrationNumber: "TX-333333", title: "ActionMan");
            testDbContext.CopyrightRegistrations.Add(existingCopyrightEntity);
            await testDbContext.SaveChangesAsync();

            var copyrightRepo = new CopyrightRepository(testDbContext);
            var userCopyrightRepo = new UserCopyrightRepository(testDbContext);
            ICopyrightService service = new CopyrightService(copyrightRepo, userCopyrightRepo);

            var createCopyrightDTO = new CopyrightCreateDTO
            {
                RegistrationNumber = "TX-333333",
                WorkType = CopyrightWorkType.Audiovisual,
                Title = "ActionMan (ignored)",
                Owner = "The Butcher"
            };

            var returnedEntityPublicId = await service.CreateAsync(user.Id, createCopyrightDTO, CancellationToken.None);

            (await testDbContext.CopyrightRegistrations.CountAsync(c => c.RegistrationNumber == "TX-333333"))
                .Should().Be(1);

            returnedEntityPublicId.Should().Be(existingCopyrightEntity.PublicId);

            var link = await testDbContext.UserCopyrights
                .SingleAsync(l => l.ApplicationUserId == user.Id && l.CopyrightRegistrationId == existingCopyrightEntity.Id);

            link.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task GetDetailsAsync_WhenLinked_ReturnsDetailsDTO_WithStoredTypeOfWorkString()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u4", UserName = "user4", Email = "u4@test.com" };
            testDbContext.Users.Add(user);


            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright(
                registrationNumber: "TX-444444",
                title: "Delta Force",
                typeOfWork: CopyrightWorkType.VisualArts.ToString(),
                owner: "Owner D");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).AddOrUndeleteAsync(user.Id, copyrightEntity.Id);

            var copyrightService = new CopyrightService(new CopyrightRepository(testDbContext), new UserCopyrightRepository(testDbContext));

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(user.Id, copyrightEntity.PublicId, CancellationToken.None);

            copyrightDetailsDTO.Should().NotBeNull();
            copyrightDetailsDTO!.Title.Should().Be("Delta Force");
            copyrightDetailsDTO.TypeOfWork.Should().Be("VisualArts");
        }

        [Test]
        public async Task RemoveAsync_WhenLinked_ReturnsTrue_AndSoftDeletes()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser { Id = "u5", UserName = "user5", Email = "u5@test" };
            testDbContext.Users.Add(user);

            var copyrightEntity = InMemoryDbContextFactory.CreateCopyright(registrationNumber: "TX-555555", title: "Titanic 1000");

            testDbContext.CopyrightRegistrations.Add(copyrightEntity);
            await testDbContext.SaveChangesAsync();

            await new UserCopyrightRepository(testDbContext).AddOrUndeleteAsync(user.Id, copyrightEntity.Id);

            var copyrightService = new CopyrightService(new CopyrightRepository(testDbContext), new UserCopyrightRepository(testDbContext));

            var isLinkSoftRemoved = await copyrightService.RemoveAsync(user.Id, copyrightEntity.PublicId, CancellationToken.None);

            isLinkSoftRemoved.Should().BeTrue();

            var link = await testDbContext.UserCopyrights.
                SingleAsync(l => l.ApplicationUserId == user.Id && l.CopyrightRegistrationId == copyrightEntity.Id);

            link.IsDeleted.Should().BeTrue();
        }
    }
}
