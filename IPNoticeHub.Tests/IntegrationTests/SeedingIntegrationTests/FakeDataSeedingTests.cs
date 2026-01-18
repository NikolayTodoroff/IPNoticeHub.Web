using FluentAssertions;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Seeding;
using IPNoticeHub.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.IdentityConstants.DemoUserCredentials;

namespace IPNoticeHub.Tests.IntegrationTests.SeedingIntegrationTests
{
    public class FakeDataSeedingTests
    {
        private SqliteConnection dbConnection = null!;
        private ServiceProvider serviceProvider = null!;

        [SetUp]
        public void SetUp()
        {
            dbConnection = new SqliteConnection("Filename=:memory:");
            dbConnection.Open();

            var services = new ServiceCollection();

            services.AddLogging(b => b.AddDebug().AddConsole());

            services.AddDbContext<IPNoticeHubDbContext>(options =>
            {
                options.UseSqlite(dbConnection);
            });

            services.AddIdentityCore<Infrastructure.Identity.ApplicationUser>(options => { }).
                AddEntityFrameworkStores<IPNoticeHubDbContext>();

            serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = 
                scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            dbContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            serviceProvider.Dispose();
            dbConnection.Dispose();
        }

        [Test]
        public async Task SeedFakeDataAsync_WhenCalledTwice_SeedsOnceAndCreatesSingleMarker()
        {
            using var scope1 = serviceProvider.CreateScope();
            var serviceProvider1 = scope1.ServiceProvider;
            await EnsureDemoUserExistsAsync(serviceProvider1);

            await FakeSeedingExtensions.SeedFakeDataAsync(
                scopedServices: serviceProvider1,
                environmentName: "Development",
                seedingEnabled: true);

            int trademarksAfterFirstRun;
            int copyrightsAfterFirstRun;

            using (var verifyScope1 = serviceProvider.CreateScope())
            {
                var dbContext = 
                    verifyScope1.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                dbContext.Set<SeedHistoryEntry>().Count().Should().Be(1);

                trademarksAfterFirstRun = dbContext.TrademarkRegistrations.Count();
                copyrightsAfterFirstRun = dbContext.CopyrightRegistrations.Count();

                trademarksAfterFirstRun.Should().BeGreaterThan(0);
                copyrightsAfterFirstRun.Should().BeGreaterThan(0);
            }

            using var scope2 = serviceProvider.CreateScope();
            var serviceProvider2 = scope2.ServiceProvider;

            await FakeSeedingExtensions.SeedFakeDataAsync(
                scopedServices: serviceProvider2,
                environmentName: "Development",
                seedingEnabled: true);

            using (var verifyScope2 = serviceProvider.CreateScope())
            {
                var dbContext = 
                    verifyScope2.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                dbContext.Set<SeedHistoryEntry>().Count().Should().Be(1);
                dbContext.TrademarkRegistrations.Count().Should().Be(trademarksAfterFirstRun);
                dbContext.CopyrightRegistrations.Count().Should().Be(copyrightsAfterFirstRun);
            }
        }

        private static async Task EnsureDemoUserExistsAsync(IServiceProvider serviceProvider)
        {
            var userManager = 
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string demoUserEmail = DemoUserEmailAddress;

            var userExists = await userManager.FindByEmailAsync(demoUserEmail);
            if (userExists != null) return;

            var demoUser = new ApplicationUser
            {
                UserName = demoUserEmail,
                Email = demoUserEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(demoUser);
            result.Succeeded.Should().BeTrue();
        }

        [Test]
        public async Task SeedFakeDataAsync_CreatesExpectedMarkerValues()
        {
            using var scope = this.serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            await FakeSeedingExtensions.SeedFakeDataAsync(
                scopedServices: serviceProvider,
                environmentName: "Development",
                seedingEnabled: true);

            using var verifyScope = this.serviceProvider.CreateScope();

            var dbContext = 
                verifyScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            var seedHistoryMarker = dbContext.Set<SeedHistoryEntry>().Single();

            seedHistoryMarker.SeedKey.Should().Be("FakeData");
            seedHistoryMarker.Version.Should().Be("1");
            seedHistoryMarker.Environment.Should().Be("Development");

            seedHistoryMarker.AppliedOnUtc.Should().BeCloseTo(
                DateTime.UtcNow, 
                precision: TimeSpan.FromMinutes(2));
        }

        [Test]
        public async Task SeedFakeDataAsync_WhenSeederThrows_RollsBackMarker()
        {
            using var scope = this.serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            Func<IServiceProvider, Task> failingSeeder =
                _ => throw new InvalidOperationException("Seeding failed!");

            Func<Task> seeding = async () =>
                await FakeSeedingExtensions.SeedFakeDataAsync(
                    scopedServices: serviceProvider,
                    environmentName: "Development",
                    seedingEnabled: true,
                    seedAction: failingSeeder);

            await seeding.Should().ThrowAsync<InvalidOperationException>();

            using var verifyScope = this.serviceProvider.CreateScope();

            var dbContext = 
                verifyScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            dbContext.Set<SeedHistoryEntry>().Should().BeEmpty();
        }
    }
}
