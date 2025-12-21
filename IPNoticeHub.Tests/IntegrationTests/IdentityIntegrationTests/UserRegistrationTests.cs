using FluentAssertions;
using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using static IPNoticeHub.Shared.Support.RoleNames;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;

namespace IPNoticeHub.Tests.IntegrationTests.IdentityIntegrationTests
{
    public class UserRegistrationTests
    {
        [Test]
        public async Task UserRegistrationService_Assigns_User_Role_To_New_Users()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            var roleManager = 
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            await EnsureRoleAsync(roleManager, User);

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var logger = 
                scope.ServiceProvider.GetRequiredService<ILogger<UserRegistrationService>>();

            var service = new UserRegistrationService(userManager, logger);

            var user1Reg = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                    "user1@test.com", 
                    "Password123!"));

            var user2Reg = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                    "user2@test.com", 
                    "Password456!"));

            user1Reg.Succeeded.Should().BeTrue();
            user2Reg.Succeeded.Should().BeTrue();

            var user1 =
                    await userManager.FindByEmailAsync("user1@test.com");

            var user2 =
                await userManager.FindByEmailAsync("user2@test.com");

            (await userManager.IsInRoleAsync(user1!, User))
                .Should().BeTrue();

            (await userManager.IsInRoleAsync(user2!, User))
                .Should().BeTrue();

            (await userManager.IsInRoleAsync(user1!, Admin))
                .Should().BeFalse();

            (await userManager.IsInRoleAsync(user2!, Admin))
                .Should().BeFalse();
        }

        [Test]
        public async Task Should_Not_Duplicate_User_Role_For_Non_Admin_Users()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await EnsureRoleAsync(roleManager, User);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var logger =
                scope.ServiceProvider.GetRequiredService<ILogger<UserRegistrationService>>();

            var service = new UserRegistrationService(userManager, logger);

            var user = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                    "user@test.com",
                    "Password123!"));

            var updatedUser =
                await userManager.FindByEmailAsync("user@test.com");

            var roles = await userManager.GetRolesAsync(updatedUser!);

            roles.Should().Contain(User);
            roles.Count(r => r == User).Should().Be(1);
        }

        [Test]
        public async Task RegisterUserAsync_ReturnsFailure_When_CreateUser_Fails()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var logger = 
                scope.ServiceProvider.GetRequiredService<ILogger<UserRegistrationService>>();

            var service = new UserRegistrationService(userManager, logger);

            var result = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                Email: "badpass@test.com",
                Password: "12"
            ));

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();

            var createdUser = 
                await userManager.FindByEmailAsync("badpass@test.com");

            createdUser.Should().BeNull();
        }

        [Test]
        public async Task RegisterUserAsync_DeletesUser_And_ThrowsException_When_RoleDoesNotExist()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var logger = 
                new TestLogger<UserRegistrationService>();

            var service = new UserRegistrationService(userManager, logger);

            var email = "norole@test.com";

            var result = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                Email: email,
                Password: "Password123!"
            ));

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();

            var userAfter = await userManager.FindByEmailAsync(email);
            userAfter.Should().BeNull();

            logger.Entries.Any(
                e => e.level == LogLevel.Critical && 
            e.message.Contains("failed due to an exception")).Should().BeTrue();
        }

        [Test]
        public async Task RegisterUserAsync_DeletesUser_And_ReturnsFailure_When_AddToRole_ReturnsFailedResult()
        {
            var userManager = UserManagerMockFactory.MockUserManager();

            var logger = new TestLogger<UserRegistrationService>();

            userManager.Setup(m => m.CreateAsync(
                It.IsAny<ApplicationUser>(), 
                It.IsAny<string>())).
                ReturnsAsync(IdentityResult.Success);

            userManager.Setup(m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(), 
                User)).
                ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Role assign failed" }));

            userManager.Setup(
                m => m.DeleteAsync(It.IsAny<ApplicationUser>())).
                ReturnsAsync(IdentityResult.Success);

            var service = 
                new UserRegistrationService(userManager.Object, logger);

            var result = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                Email: "user@test.com",
                Password: "Password123!"
            ));

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("Role assign failed");

            userManager.Verify(m => m.DeleteAsync(
                It.IsAny<ApplicationUser>()), 
                Times.Once);

            logger.Entries.Any(e => e.level == LogLevel.Critical && 
            e.message.Contains("failed to assign role")).Should().BeTrue();
        }

        [Test]
        public async Task RegisterUserAsync_LogsCritical_When_DeleteFails_After_RoleAssignmentFailure()
        {
            var userManager = 
                UserManagerMockFactory.MockUserManager();

            var logger = 
                new TestLogger<UserRegistrationService>();

            var service = 
                new UserRegistrationService(userManager.Object, logger);

            userManager.Setup(m => m.CreateAsync(
                It.IsAny<ApplicationUser>(), 
                It.IsAny<string>())).
                ReturnsAsync(IdentityResult.Success);

            userManager.Setup(m => m.AddToRoleAsync(
                It.IsAny<ApplicationUser>(), 
                User)).
                ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Adding a role failed" }));

            userManager.Setup(
                m => m.DeleteAsync(It.IsAny<ApplicationUser>())).
                ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "User Deletion fail" }));

            var result = await service.RegisterUserAsync(
                new UserRegistrationRequest(
                Email: "x@test.com",
                Password: "Password123!"
            ));

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("Adding a role failed");

            logger.Entries.Any(e => e.level == LogLevel.Critical &&
            e.message.Contains("Failed to delete orphaned user")).Should().BeTrue();

            logger.Entries.Any(e => e.level == LogLevel.Critical &&
            e.message.Contains("failed to assign role")).Should().BeTrue();
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roles, string roleName)
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var create = await roles.CreateAsync(new IdentityRole(roleName));
                create.Succeeded.Should().BeTrue($"Role '{roleName}' must exist");
            }
        }
    }
}
