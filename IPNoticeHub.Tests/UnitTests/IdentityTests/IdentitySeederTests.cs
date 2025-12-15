using FluentAssertions;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Tests.UnitTests.UnitTestUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using static IPNoticeHub.Shared.Support.RoleNames;

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

            (await roleManager.RoleExistsAsync(Admin)).
                Should().
                BeTrue();

            (await roleManager.RoleExistsAsync(User)).
                Should(). 
                BeTrue();
        }

        [Test]
        public async Task Should_Create_Admin_User()
        {
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
            using var host = new IdentityTestHost();
            using var scope = host.CreateScope();

            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);
            await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

        //[Test]
        //public async Task Should_Confirm_Email_For_Existing_Admin_If_Not_Confirmed()
        //{
        //    using var host = new IdentityTestHost();
        //    using var scope = host.CreateScope();

        //    await IdentitySeeder.SeedIdentitiesAsync(scope.ServiceProvider);

        //    var userManager =
        //        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var adminUser = new ApplicationUser
        //    {
        //        UserName = AdminEmailAddress,
        //        Email = AdminEmailAddress,
        //        EmailConfirmed = false
        //    };

        //    await userManager.CreateAsync(adminUser, AdminEmailPassword);

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var updatedAdmin =
        //        await userManager.FindByEmailAsync(AdminEmailAddress);

        //    updatedAdmin.Should().
        //        NotBeNull();

        //    updatedAdmin!.EmailConfirmed.Should().
        //        BeTrue("seeder should confirm email for existing admin");
        //}

        //[Test]
        //public async Task Should_Add_Password_To_Existing_Admin_Without_Password()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var adminUser = new ApplicationUser
        //    {
        //        UserName = AdminEmailAddress,
        //        Email = AdminEmailAddress,
        //        EmailConfirmed = true
        //    };

        //    await userManager.CreateAsync(adminUser);

        //    (await userManager.HasPasswordAsync(adminUser)).Should().
        //        BeFalse("admin should not have password initially");

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var updatedAdmin =
        //        await userManager.FindByEmailAsync(AdminEmailAddress);

        //    (await userManager.HasPasswordAsync(updatedAdmin!)).Should().
        //        BeTrue("seeder should add password to existing admin");

        //    (await userManager.CheckPasswordAsync(updatedAdmin!, AdminEmailPassword)).Should().
        //        BeTrue("the added password should match the configured admin password");
        //}

        //[Test]
        //public async Task Should_Add_Admin_Role_To_Existing_Admin_User_Without_Admin_Role()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var roleManager =
        //        provider.GetRequiredService<RoleManager<IdentityRole>>();

        //    await roleManager.CreateAsync(new IdentityRole(Admin));
        //    await roleManager.CreateAsync(new IdentityRole(User));

        //    var adminUser = new ApplicationUser
        //    {
        //        UserName = AdminEmailAddress,
        //        Email = AdminEmailAddress,
        //        EmailConfirmed = true
        //    };

        //    await userManager.CreateAsync(adminUser, AdminEmailPassword);

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var updatedAdmin =
        //        await userManager.FindByEmailAsync(AdminEmailAddress);

        //    (await userManager.IsInRoleAsync(updatedAdmin!, Admin)).Should().
        //        BeTrue("seeder should add Admin role to existing admin user");

        //    (await userManager.IsInRoleAsync(updatedAdmin!, User)).Should().
        //        BeTrue("seeder should add User role to existing admin user");
        //}

        //[Test]
        //public async Task Should_Assign_User_Role_To_All_Non_Admin_Users()
        //{
        //    using var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var regularUser1 = new ApplicationUser
        //    {
        //        UserName = "user1@test.com",
        //        Email = "user1@test.com",
        //        EmailConfirmed = true
        //    };

        //    var regularUser2 = new ApplicationUser
        //    {
        //        UserName = "user2@test.com",
        //        Email = "user2@test.com",
        //        EmailConfirmed = true
        //    };

        //    await userManager.CreateAsync(
        //        regularUser1, 
        //        "Password123!");

        //    await userManager.CreateAsync(
        //        regularUser2, 
        //        "Password123!");

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var user1 = await userManager.
        //        FindByEmailAsync("user1@test.com");

        //    var user2 = await userManager.
        //        FindByEmailAsync("user2@test.com");

        //    (await userManager.IsInRoleAsync(user1!, User)).Should().
        //        BeTrue(
        //        "regular user 1 should have User role");

        //    (await userManager.IsInRoleAsync(user2!, User)).Should().
        //        BeTrue(
        //        "regular user 2 should have User role");

        //    (await userManager.IsInRoleAsync(user1!, Admin)).Should().
        //        BeFalse(
        //        "regular user 1 should not have Admin role");

        //    (await userManager.IsInRoleAsync(user2!, Admin)).Should().
        //        BeFalse(
        //        "regular user 2 should not have Admin role");
        //}

        //[Test]
        //public async Task Should_Not_Duplicate_User_Role_For_Non_Admin_Users()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var roleManager =
        //        provider.GetRequiredService<RoleManager<IdentityRole>>();

        //    await roleManager.CreateAsync(new IdentityRole(User));

        //    var regularUser = new ApplicationUser
        //    {
        //        UserName = "user@test.com",
        //        Email = "user@test.com",
        //        EmailConfirmed = true
        //    };

        //    await userManager.CreateAsync(regularUser, "Password123!");
        //    await userManager.AddToRoleAsync(regularUser, User);

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);
        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var updatedUser = await userManager.FindByEmailAsync("user@test.com");
        //    var roles = await userManager.GetRolesAsync(updatedUser!);

        //    roles.Should().Contain(User);

        //    roles.Count(r => r == User).Should().
        //        Be(1, "User role should not be duplicated");
        //}

        //[Test]
        //public async Task Should_Skip_Adding_User_Role_To_Admin_Users()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var roleManager =
        //        provider.GetRequiredService<RoleManager<IdentityRole>>();

        //    await roleManager.CreateAsync(new IdentityRole(Admin));
        //    await roleManager.CreateAsync(new IdentityRole(User));

        //    var adminUser = new ApplicationUser
        //    {
        //        UserName = "anotheradmin@test.com",
        //        Email = "anotheradmin@test.com",
        //        EmailConfirmed = true
        //    };

        //    await userManager.CreateAsync(adminUser, "Password123!");
        //    await userManager.AddToRoleAsync(adminUser, Admin);

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var updatedUser = await userManager.FindByEmailAsync(
        //        "anotheradmin@test.com");

        //    updatedUser.Should().
        //        NotBeNull("admin user should still exist after seeding");

        //    (await userManager.IsInRoleAsync(updatedUser!, Admin)).Should().
        //        BeTrue("admin user should retain Admin role");
        //}

        //[Test]
        //public async Task Should_Handle_Empty_User_List()
        //{
        //    using var provider = IdentityTestFactory.BuildIdentityServiceProvider();

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    using var scope = provider.CreateScope();
        //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var allUsers = await userManager.Users.ToListAsync();

        //    allUsers.Should().HaveCount(1);
        //    allUsers.Single().Email.Should().Be(AdminEmailAddress);
        //}


        //[Test]
        //public async Task Should_Create_Admin_With_Correct_Username_And_Email()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var userManager =
        //        provider.GetRequiredService<UserManager<ApplicationUser>>();

        //    var admin =
        //        await userManager.FindByEmailAsync(AdminEmailAddress);

        //    admin.Should().NotBeNull();

        //    admin!.UserName.Should().
        //        Be(AdminEmailAddress, "username should match email");

        //    admin.Email.Should().Be(AdminEmailAddress);
        //}

        //[Test]
        //public async Task Should_Not_Skip_Roles_If_Already_Exist()
        //{
        //    var provider =
        //        IdentityTestFactory.BuildIdentityServiceProvider();

        //    var roleManager =
        //        provider.GetRequiredService<RoleManager<IdentityRole>>();

        //    await roleManager.CreateAsync(new IdentityRole(Admin));
        //    await roleManager.CreateAsync(new IdentityRole(User));

        //    await IdentitySeeder.SeedIdentitiesAsync(provider);

        //    var allRoles = roleManager.Roles.ToList();

        //    allRoles.Should().HaveCount(2);

        //    allRoles.Select(r => r.Name).Should().
        //        Contain(new[] { Admin, User });
        //}
    }
}
