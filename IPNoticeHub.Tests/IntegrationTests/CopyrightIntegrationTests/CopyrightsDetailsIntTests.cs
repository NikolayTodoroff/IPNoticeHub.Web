using System.Net;
using FluentAssertions;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests
{
    public class CopyrightsDetailsIntTests
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
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                // Seeds the user into the database
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                // Seeds the copyright entity into the database, and links it to the user
                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-DET-200",
                    typeOfWork: "Literary",
                    title: "Details Title");

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
                publicId = entity.PublicId;
            }

            var response = await client.GetAsync($"/Copyrights/Details/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetCopyrightDetails_WhenNotLinkedToUser_Returns404()
        {
            var userId = "targetUserId";
            var randomUserId = "randomUserId";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
                await TestDbSeeder.SeedUserAsync(testDbContext, randomUserId);

                // Seeds the copyright entity into the database, and links it to the user
                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-DET-404",
                    typeOfWork: "Literary",
                    title: "Unlinked Title");

                // Links the entity to a random user, not the target user
                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
                publicId = entity.PublicId;
            }

            var response = await client.GetAsync($"/Copyrights/Details/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetCopyrightDetails_WithMissingEntityId_Returns404()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            var missingId = Guid.NewGuid();

            var response = await client.GetAsync($"/Copyrights/Details/{missingId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetCopyrightDetails_Unauthenticated_Returns401()
        {
            // No user: CreateClientAs() is not called
            var client = appFactory.CreateClient();

            var anyId = Guid.NewGuid();
            var response = await client.GetAsync($"/Copyrights/Details/{anyId}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
