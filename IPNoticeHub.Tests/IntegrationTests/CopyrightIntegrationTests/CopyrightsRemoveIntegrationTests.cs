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
    public class CopyrightsRemoveIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Post_RemoveCopyrights_WhenLinked_RedirectsToMyCollection_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid copyrightEntityPublicId;
            int copyrightEntityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-302",
                    typeOfWork: "Literary",
                    title: "Entity For Removal"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync($"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(new Dictionary<string, string?>()));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Copyrights/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.Set<UserCopyright>().AsNoTracking().
                    FirstOrDefaultAsync(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightEntityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveCopyrights_WithLocalReturnUrl_RedirectsToReturnUrl_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid copyrightEntityPublicId;
            int copyrightEntityId;
            const string returnUrl = "/Copyrights/MyCollection?page=3&sortBy=DateAdded";

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-RET",
                    typeOfWork: "Literary",
                    title: "Remove With ReturnUrl"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync($"/Copyrights/Remove/{copyrightEntityPublicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.PathAndQuery.Should().Be(returnUrl);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.Set<UserCopyright>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightEntityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Remove_WithExternalReturnUrl_IgnoresReturnUrl_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid copyrightEntityPublicId;
            int copyrightEntityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-EXT",
                    typeOfWork: "Literary",
                    title: "Remove With External ReturnUrl"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["returnUrl"] = "https://example.com/redirect-away" // should be ignored
            };

            var response = await client.PostAsync($"/Copyrights/Remove/{copyrightEntityPublicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Copyrights/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.Set<UserCopyright>().AsNoTracking()
                    .FirstOrDefaultAsync(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightEntityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveCopyright_WithUnauthenticatedUser_Returns401_AndNoChanges()
        {
            var client = appFactory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            Guid copyrightEntityPublicId;
            int copyrightEntityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-401",
                    typeOfWork: "Literary",
                    title: "Remove Unauthorized");

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync($"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(new Dictionary<string, string?>()));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                bool entityStillExists = await testDbContext.CopyrightRegistrations.AsNoTracking()
                    .AnyAsync(c => c.Id == copyrightEntityId);

                entityStillExists.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveCopyright_WhenNotLinked_RedirectsToMyCollection_AndDoesNotModifyOtherLinks()
        {
            var targetUserId = "u1";
            var randomUserId = "u2";
            var client = appFactory.CreateClientAs(targetUserId);

            Guid copyrightEntityPublicId;
            int copyrightEntityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, targetUserId);
                await TestDbSeeder.SeedUserAsync(testDbContext, randomUserId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-NOTLINK",
                    typeOfWork: "Literary",
                    title: "Remove Not Linked"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync($"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(new Dictionary<string, string?>()));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.AbsolutePath.Should().Be("/Copyrights/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var targetUserLink = await testDbContext.Set<UserCopyright>().AsNoTracking()
                    .FirstOrDefaultAsync(uc => uc.ApplicationUserId == targetUserId && uc.CopyrightRegistrationId == copyrightEntityId);

                targetUserLink.Should().BeNull("The target entity was never linked; nothing to delete");

                var randomUserLink = await testDbContext.Set<UserCopyright>().AsNoTracking()
                    .FirstOrDefaultAsync(uc => uc.ApplicationUserId == randomUserId && uc.CopyrightRegistrationId == copyrightEntityId);

                randomUserLink.Should().NotBeNull();
                randomUserLink!.IsDeleted.Should().BeFalse("The unrelated user's link must not be altered");
            }
        }
    }
}
