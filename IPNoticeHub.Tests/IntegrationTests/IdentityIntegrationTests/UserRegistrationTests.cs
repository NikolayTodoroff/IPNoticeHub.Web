using FluentAssertions;
using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Shared.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using static IPNoticeHub.Shared.Support.RoleNames;

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

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roles, string roleName)
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var create = await roles.CreateAsync(new IdentityRole(roleName));
                create.Succeeded.Should().BeTrue($"role '{roleName}' must exist");
            }
        }
    }
}
