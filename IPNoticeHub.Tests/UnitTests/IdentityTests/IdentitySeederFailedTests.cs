using FluentAssertions;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Shared.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.IdentityConstants.AdminAccountCredentials;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;

namespace IPNoticeHub.Tests.UnitTests.IdentityTests
{
    public class IdentitySeederFailedTests
    {
        [Test]
        public async Task Seeder_ReturnsEarly_When_RoleCreation_Fails_And_DoesNot_Touch_UserManager()
        {
            var roleManager = 
                IdentityManagerMocksFactory.MockRoleManager();

            var userManager = 
                IdentityManagerMocksFactory.MockUserManager();

            roleManager.Setup(
                r => r.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(false);

            roleManager.Setup(
                r => r.CreateAsync(
                    It.IsAny<IdentityRole>())).ReturnsAsync(
                IdentityResult.Failed(
                    new IdentityError { Description = "boom" }));

            var logger = new TestLogger<IdentitySeeder>();

            using var serviceProvider = BuildServicesForSeederUnitTest(services =>
            {
                services.AddScoped(_ => roleManager.Object);
                services.AddScoped(_ => userManager.Object);
                services.AddScoped<ILogger<IdentitySeeder>>(_ => logger);
            });

            await IdentitySeeder.SeedIdentitiesAsync(serviceProvider);

            userManager.Verify(
                u => u.FindByEmailAsync(
                    It.IsAny<string>()), 
                Times.Never);

            logger.Entries.Any(
                e => e.level == LogLevel.Critical && 
                e.message.Contains("Failed to create role")).Should().BeTrue();
        }

        [Test]
        public async Task Seeder_LogsCritical_And_Returns_When_AdminCreation_Fails()
        {
            var roleManager =
                IdentityManagerMocksFactory.MockRoleManager();

            var userManager =
                IdentityManagerMocksFactory.MockUserManager();

            roleManager.Setup(
                r => r.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(true);

            userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).
                ReturnsAsync((ApplicationUser?)null);

            userManager.Setup(
                u => u.CreateAsync(
                    It.IsAny<ApplicationUser>(), 
                    It.IsAny<string>())).
                    ReturnsAsync(IdentityResult.Failed(
                        new IdentityError { Description = "nope" }));

            var logger = new TestLogger<IdentitySeeder>();

            using var sp = BuildServicesForSeederUnitTest(services =>
            {
                services.AddScoped(_ => roleManager.Object);
                services.AddScoped(_ => userManager.Object);
                services.AddScoped<ILogger<IdentitySeeder>>(_ => logger);
            });

            await IdentitySeeder.SeedIdentitiesAsync(sp);

            userManager.Verify(
                u => u.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(), 
                    It.IsAny<string>()), 
                Times.Never);
    
            logger.Entries.Any(e => e.level == LogLevel.Critical && 
            e.message.Contains("Failed to create admin")).Should().BeTrue();

            userManager.Verify( u => u.CreateAsync(
                   It.IsAny<ApplicationUser>(), 
                   It.IsAny<string>()),
                   Times.Once);
        }

        [Test]
        public async Task Seeder_LogsWarning_When_AddPassword_Fails_But_Still_Assigns_Roles()
        {
            var roleManager =
                IdentityManagerMocksFactory.MockRoleManager();

            var userManager =
                IdentityManagerMocksFactory.MockUserManager();

            roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                       .ReturnsAsync(true);

            var existingAdmin = new ApplicationUser { 
                Email = AdminEmailAddress, 
                UserName = AdminEmailAddress, 
                EmailConfirmed = true };

            userManager.Setup(
                u => u.FindByEmailAsync(AdminEmailAddress)).
                ReturnsAsync(existingAdmin);

            userManager.Setup(
                u => u.HasPasswordAsync(existingAdmin)).
                ReturnsAsync(false);

            userManager.Setup(
                u => u.AddPasswordAsync(existingAdmin, It.IsAny<string>())).
                ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "pwd fail" }));

            userManager.Setup(
                u => u.IsInRoleAsync(existingAdmin, It.IsAny<string>())).
                ReturnsAsync(false);

            userManager.Setup(
                u => u.AddToRoleAsync(existingAdmin, It.IsAny<string>())).
                ReturnsAsync(IdentityResult.Success);

            var logger = new TestLogger<IdentitySeeder>();

            using var sp = BuildServicesForSeederUnitTest(services =>
            {
                services.AddScoped(_ => roleManager.Object);
                services.AddScoped(_ => userManager.Object);
                services.AddScoped<ILogger<IdentitySeeder>>(_ => logger);
            });

            await IdentitySeeder.SeedIdentitiesAsync(sp);

            logger.Entries.Any(e => e.level == LogLevel.Warning && 
            e.message.Contains("Failed to add password to existing admin")).Should().BeTrue();

            userManager.Verify(
                u => u.AddToRoleAsync(
                    existingAdmin, 
                    RoleNames.Admin), 
                Times.Once);
            
            userManager.Verify(
                u => u.AddToRoleAsync(
                    existingAdmin, 
                    RoleNames.User), 
                Times.Once);
        }

        private static ServiceProvider BuildServicesForSeederUnitTest(
            Action<IServiceCollection> overrides)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            overrides(services);
            return services.BuildServiceProvider();
        }
    }   
}
