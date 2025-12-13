using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.UnitTestUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using static IPNoticeHub.Shared.Support.RoleNames;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using IPNoticeHub.Infrastructure.Identity;

namespace IPNoticeHub.Tests.UnitTests.IdentityTests
{
    public class IdentitySeederTests
    {
        [Test]
        public async Task Should_Create_Admin_And_User_Roles()
        {
            var provider = 
                IdentityTestFactory.BuildIdentityServiceProvider();

            await IdentitySeeder.SeedIdentitiesAsync(provider);

            var roleManager = 
                provider.GetRequiredService<RoleManager<IdentityRole>>();

            (await roleManager.RoleExistsAsync(Admin)).Should().
                BeTrue();

            (await roleManager.RoleExistsAsync(User)).Should().
                BeTrue();
        }

        [Test]
        public async Task Should_Create_Admin_User()
        {
            var provider = IdentityTestFactory.BuildIdentityServiceProvider();

            await IdentitySeeder.SeedIdentitiesAsync(provider);

            var userManager = 
                provider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = 
                await userManager.FindByEmailAsync(AdminEmailAddress);

            admin.Should().
                NotBeNull();

            admin!.Email.Should().
                Be(AdminEmailAddress);

            admin.EmailConfirmed.Should().
                BeTrue("the seeder explicitly confirms admin email");
        }

        [Test]
        public async Task Admin_Should_Be_Assigned_To_Admin_And_User_Roles()
        {
            var provider = 
                IdentityTestFactory.BuildIdentityServiceProvider();

            await IdentitySeeder.SeedIdentitiesAsync(provider);

            var userManager = 
                provider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = 
                await userManager.FindByEmailAsync(AdminEmailAddress);

            admin.Should().
                NotBeNull();

            (await userManager.IsInRoleAsync(admin!, Admin)).Should()
                .BeTrue("admin must have Admin role");

            (await userManager.IsInRoleAsync(admin!, User)).Should()
                .BeTrue("admin must also belong to User role for full access");
        }

        [Test]
        public async Task Seeder_Should_Be_Idempotent_When_Run_Multiple_Times()
        {
            var provider = 
                IdentityTestFactory.BuildIdentityServiceProvider();

            await IdentitySeeder.SeedIdentitiesAsync(provider);
            await IdentitySeeder.SeedIdentitiesAsync(provider);

            var roleManager = 
                provider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = 
                provider.GetRequiredService<UserManager<ApplicationUser>>();

            var allRoles = roleManager.Roles.ToList();

            allRoles.Should().
                HaveCount(2);

            allRoles.Select(r => r.Name).Should().
                Contain(new[] { Admin, User });

            var adminUsers = userManager.Users
                .Where(u => u.Email == AdminEmailAddress)
                .ToList();

            adminUsers.Should().
                HaveCount(1);

            var admin = adminUsers.Single();

            (await userManager.IsInRoleAsync(admin, Admin)).Should().
                BeTrue();

            (await userManager.IsInRoleAsync(admin, User)).Should().
                BeTrue();
        }
    }
}
