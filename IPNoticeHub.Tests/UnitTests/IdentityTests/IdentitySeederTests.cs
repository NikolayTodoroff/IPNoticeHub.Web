using FluentAssertions;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;
using static IPNoticeHub.Shared.Support.RoleNames;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;

namespace IPNoticeHub.Tests.UnitTests.IdentityTests
{
    public class IdentitySeederTests
    {
        [Test]
        public async Task Should_Create_Admin_And_User_Roles()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            (await roleManager.RoleExistsAsync(Admin)).Should().BeTrue();
            (await roleManager.RoleExistsAsync(User)).Should().BeTrue();
        }

        [Test]
        public async Task Should_Create_Admin_User()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await userManager.FindByEmailAsync(AdminEmailAddress);

            admin.Should().NotBeNull();
            admin!.Email.Should().Be(AdminEmailAddress);

            admin.EmailConfirmed.Should().BeTrue();
        }

        [Test]
        public async Task DefaultAdmin_Should_Be_Assigned_To_Admin_And_User_Roles()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await userManager.FindByEmailAsync(AdminEmailAddress);

            admin.Should().NotBeNull();
            (await userManager.IsInRoleAsync(admin!, Admin)).Should().BeTrue();
            (await userManager.IsInRoleAsync(admin!, User)).Should().BeTrue();
        }

        [Test]
        public async Task Seeder_Should_Be_Idempotent_When_Run_Multiple_Times()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);
            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var allRoles = roleManager.Roles.ToList();

            allRoles.Should().HaveCount(2);

            allRoles.Select(r => r.Name).Should().Contain(new[] { Admin, User });

            var adminUsers = userManager.Users
                .Where(u => u.Email == AdminEmailAddress)
                .ToList();

            adminUsers.Should().HaveCount(1);

            var admin = adminUsers.Single();

            (await userManager.IsInRoleAsync(admin, Admin)).Should().BeTrue();
            (await userManager.IsInRoleAsync(admin, User)).Should().BeTrue();
        }

        [Test]
        public async Task IdentitySeeder_Should_Confirm_Email_For_Preexisting_Admin()
        {
            using var host = new IdentityTestHost();

            using (var scope = host.CreateScope())
            {
                var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var admin = new ApplicationUser
                {
                    UserName = AdminEmailAddress,
                    Email = AdminEmailAddress,
                    EmailConfirmed = false
                };

                var create = await userManager.CreateAsync(admin, AdminEmailPassword);
                create.Succeeded.Should().BeTrue();

                await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);
            } ;

            using (var scope = host.CreateScope())
            {
                var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var updated = await userManager.FindByEmailAsync(AdminEmailAddress);
                updated!.EmailConfirmed.Should().BeTrue();
            }        
        }

        [Test]
        public async Task IdentitySeeder_Should_Add_Password_For_Preexisting_Admin_Without_Password()
        {
            using var host = new IdentityTestHost();
            
            using (var scope = host.CreateScope())
            {
                var userManager = 
                    scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var admin = new ApplicationUser { 
                    UserName = AdminEmailAddress, 
                    Email = AdminEmailAddress, 
                    EmailConfirmed = true 
                };

                var create = await userManager.CreateAsync(admin);
                create.Succeeded.Should().BeTrue();

                (await userManager.HasPasswordAsync(admin)).Should().BeFalse();

                await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);
            }

            using (var scope = host.CreateScope())
            {
                var userManager = 
                    scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var updated = await userManager.FindByEmailAsync(AdminEmailAddress);

                (await userManager.HasPasswordAsync(updated!)).Should().BeTrue();
                (await userManager.CheckPasswordAsync(updated!, AdminEmailPassword)).Should().BeTrue();
            }
        }

        [Test]
        public async Task Should_Seed_Only_Admin_User_When_No_Other_Users_Exist()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var allUsersBefore = await userManager.Users.ToListAsync();
            allUsersBefore.Should().BeEmpty();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var allUsersAfter = await userManager.Users.ToListAsync();
            allUsersAfter.Should().HaveCount(2);
            allUsersAfter.Should().Contain(u => u.Email == AdminEmailAddress);
            allUsersAfter.Should().Contain(u => u.Email == DemoUserEmailAddress);
        }

        [Test]
        public async Task Should_Create_Default_Admin_With_Matching_Username_And_Email()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin =
                await userManager.FindByEmailAsync(AdminEmailAddress);

            admin.Should().NotBeNull();

            admin!.UserName.Should().Be(AdminEmailAddress);

            admin.Email.Should().Be(AdminEmailAddress);
        }
    }
}
