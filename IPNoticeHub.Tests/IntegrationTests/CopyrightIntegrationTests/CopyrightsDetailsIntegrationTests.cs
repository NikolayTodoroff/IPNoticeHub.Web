using System.Net;
using FluentAssertions;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests
{
    [NonParallelizable]
    public class CopyrightsDetailsIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task GetCopyrightDetails_WhenLinkedToUser_Returns200()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-DET-200",
                    typeOfWork: "Literary",
                    title: "Details Title");

                testDbContext.
                    Set<UserCopyright>().
                    Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
                publicId = entity.PublicId;
            }

            var response = await client.GetAsync(
                $"/Copyrights/Details/{publicId}");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetCopyrightDetails_WhenNotLinkedToUser_Returns404()
        {
            var userId = "targetUserId";
            var randomUserId = "randomUserId";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    randomUserId);

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-DET-404",
                    typeOfWork: "Literary",
                    title: "Unlinked Title");

                testDbContext.
                    Set<UserCopyright>().
                    Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
                publicId = entity.PublicId;
            }

            var response = await client.GetAsync(
                $"/Copyrights/Details/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetCopyrightDetails_WithMissingEntityId_Returns404()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);
            }

            var missingId = Guid.NewGuid();

            var response = await client.GetAsync(
                $"/Copyrights/Details/{missingId}");

            response.StatusCode.Should().
                Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetCopyrightDetails_Unauthenticated_Returns401()
        {
            var client = appFactory.CreateClient();

            var anyId = Guid.NewGuid();
            var response = await client.GetAsync(
                $"/Copyrights/Details/{anyId}");

            response.StatusCode.Should().
                Be(HttpStatusCode.Unauthorized);
        }
    }
}
