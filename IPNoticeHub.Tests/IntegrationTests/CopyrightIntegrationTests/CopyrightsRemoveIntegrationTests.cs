using FluentAssertions;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests
{
    [NonParallelizable]
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
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-302",
                    typeOfWork: "Literary",
                    title: "Entity For Removal"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync(
                $"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(
                    new Dictionary<string, string?>()));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = 
                    await testDbContext.
                    Set<UserCopyright>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                        uc => uc.ApplicationUserId == userId && 
                        uc.CopyrightEntityId == copyrightEntityId);

                link.Should().
                    NotBeNull();

                link!.IsDeleted.Should().
                    BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveCopyrights_WithLocalReturnUrl_RedirectsToReturnUrl_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid copyrightEntityPublicId;
            int copyrightEntityId;
            const string returnUrl = 
                "/Copyrights/MyCollection?page=3&sortBy=DateAdded";

            using (var serviceScope = 
                appFactory.Services.CreateScope())
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
                    regNumber: "TX-9-REM-RET",
                    typeOfWork: "Literary",
                    title: "Remove With ReturnUrl"
                );

                testDbContext.Set<UserCopyright>().Add(
                    new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var entityForm = 
                new Dictionary<string, string?>
            {
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync(
                $"/Copyrights/Remove/{copyrightEntityPublicId}", 
                new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            response.Headers.Location.Should().
                NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.PathAndQuery.Should().
                Be(returnUrl);

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.
                    Set<UserCopyright>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == copyrightEntityId);

                link.Should().NotBeNull();

                link!.IsDeleted.Should().
                    BeTrue();
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
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-EXT",
                    typeOfWork: "Literary",
                    title: "Remove With External ReturnUrl"
                );

                testDbContext.Set<UserCopyright>().
                    Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var entityForm = 
                new Dictionary<string, string?>
            {
                ["returnUrl"] = "https://example.com/redirect-away"
            };

            var response = await client.PostAsync(
                $"/Copyrights/Remove/{copyrightEntityPublicId}", 
                new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            response.Headers.Location.Should().
                NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.
                    Set<UserCopyright>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == copyrightEntityId);

                link.Should().
                    NotBeNull();

                link!.IsDeleted.Should().
                    BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveCopyright_WithUnauthenticatedUser_Returns401_AndNoChanges()
        {
            var client = appFactory.
                CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            Guid copyrightEntityPublicId;
            int copyrightEntityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-401",
                    typeOfWork: "Literary",
                    title: "Remove Unauthorized");

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync(
                $"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(
                    new Dictionary<string, string?>()));

            response.StatusCode.Should().
                Be(HttpStatusCode.Unauthorized);

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                bool entityStillExists = 
                    await testDbContext.CopyrightRegistrations.
                    AsNoTracking()
                    .AnyAsync(c => c.Id == copyrightEntityId);

                entityStillExists.Should().
                    BeTrue();
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

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    targetUserId);

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    randomUserId);

                var entity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-REM-NOTLINK",
                    typeOfWork: "Literary",
                    title: "Remove Not Linked"
                );

                testDbContext.Set<UserCopyright>().
                    Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightEntityId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                copyrightEntityPublicId = entity.PublicId;
                copyrightEntityId = entity.Id;
            }

            var response = await client.PostAsync(
                $"/Copyrights/Remove/{copyrightEntityPublicId}",
                new FormUrlEncodedContent(
                    new Dictionary<string, string?>()));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var targetUserLink = await testDbContext.
                    Set<UserCopyright>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    uc => uc.ApplicationUserId == targetUserId && 
                    uc.CopyrightEntityId == copyrightEntityId);

                targetUserLink.Should().
                    BeNull("The target entity was never linked; nothing to delete");

                var randomUserLink = await testDbContext.
                    Set<UserCopyright>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    uc => uc.ApplicationUserId == randomUserId && 
                    uc.CopyrightEntityId == copyrightEntityId);

                randomUserLink.Should().
                    NotBeNull();

                randomUserLink!.IsDeleted.Should().
                    BeFalse("The unrelated user's link must not be altered");
            }
        }

        [Test]
        public async Task Post_Remove_WithMissingEntityId_RedirectsToMyCollection_NoChange()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int copyrightId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA0001234567",
                    TypeOfWork = "Literary Work",
                    Title = "AMC",
                    YearOfCreation = 2023,
                    DateOfPublication = null,
                    Owner = "AMC LLC",
                    NationOfFirstPublication = null
                };

                testDbContext.CopyrightRegistrations.Add(entity);

                await testDbContext.SaveChangesAsync();
                copyrightId = entity.Id;

                testDbContext.Set<UserCopyright>().
                    Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = copyrightId,
                    IsDeleted = false,
                    DateAdded = DateTime.UtcNow
                });

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.PostAsync(
                "/Copyrights/Remove",
                new FormUrlEncodedContent(
                    new Dictionary<string, string?>()));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var resolvedUri = response.Headers.Location!.IsAbsoluteUri ?
               response.Headers.Location! : 
               new Uri(client.BaseAddress!, response.Headers.Location!);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");


            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = 
                    await testDbContext.UserCopyrights.
                    AsNoTracking().
                    FirstAsync(
                        uc => uc.ApplicationUserId == userId && 
                        uc.CopyrightEntityId == copyrightId);

                link.IsDeleted.Should().
                    BeFalse();
            }
        }

        [TestCase("abc", 
            TestName = "Post_Remove_NonNumericId_Redirects_NoChange")]

        [TestCase("-1", 
            TestName = "Post_Remove_NegativeId_Redirects_NoChange")]
        public async Task Post_Remove_WithInvalidId_RedirectsToMyCollection_NoChange(string invalidId)
        {
            var userId = "u1";

            var client = appFactory.
                CreateClientAs(userId);

            int entityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA0007777777",
                    TypeOfWork = "Literary Work",
                    Title = "AMZ",
                    YearOfCreation = 2024,
                    DateOfPublication = null,
                    Owner = "AAMZ LLC"
                };

                testDbContext.CopyrightRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;

                testDbContext.UserCopyrights.Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entityId,
                    IsDeleted = false,
                    DateAdded = DateTime.UtcNow
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = 
                new Dictionary<string, string?> { ["copyrightId"] = invalidId };

            var response = await client.PostAsync(
                "/Copyrights/Remove", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var resolvedUri = response.Headers.Location!.IsAbsoluteUri ?
                response.Headers.Location! : 
                new Uri(client.BaseAddress!, response.Headers.Location!);

                resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");


            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.UserCopyrights.
                    AsNoTracking().
                    FirstAsync(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == entityId);

                link.IsDeleted.Should().
                    BeFalse();
            }
        }

        [Test]
        public async Task Post_Remove_AlreadySoftDeleted_RedirectsToMyCollection_NoChange()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);
            int entityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA0008888888",
                    TypeOfWork = "Literary Work",
                    Title = "Target CR",
                    YearOfCreation = 2023,
                    Owner = "ASD LTD"
                };
                testDbContext.CopyrightRegistrations.Add(entity);

                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;

                testDbContext.UserCopyrights.Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entityId,
                    IsDeleted = true,
                    DateAdded = DateTime.UtcNow
                });

                await testDbContext.SaveChangesAsync();
            }

            var form = 
                new Dictionary<string, string?> { [
                    "copyrightId"] = entityId.ToString() };

            var response = await client.PostAsync(
                "/Copyrights/Remove", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var resolvedUri = response.Headers.Location!.IsAbsoluteUri ?
               response.Headers.Location! : 
               new Uri(client.BaseAddress!, response.Headers.Location!);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var links = 
                    await testDbContext.UserCopyrights.
                    AsNoTracking().
                    Where(
                        uc => uc.ApplicationUserId == userId && 
                        uc.CopyrightEntityId == entityId).
                        ToListAsync();

                links.Count.Should().
                    Be(1);

                links[0].IsDeleted.Should().
                    BeTrue();
            }
        }

        [Test]
        public async Task Post_Remove_WithNonExistingNumericId_RedirectsToMyCollection_NoChange()
        {
            var userId = "u1";
            var client = 
                appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA0005550000",
                    TypeOfWork = "Literary Work",
                    Title = "Target",
                    YearOfCreation = 2022,
                    Owner = "LR LTD"
                };
                testDbContext.CopyrightRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.UserCopyrights.Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = entityId,
                    IsDeleted = false,
                    DateAdded = DateTime.UtcNow
                });

                await testDbContext.SaveChangesAsync();
            }

            var form = 
                new Dictionary<string, string?> { ["copyrightId"] = "999999" };

            var response = await client.PostAsync(
                "/Copyrights/Remove", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var resolvedUri = response.Headers.Location!.IsAbsoluteUri ?
                response.Headers.Location! : 
                new Uri(client.BaseAddress!, response.Headers.Location!);

            resolvedUri.AbsolutePath.Should().
                Be("/Copyrights/MyCollection");


            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var totalLinks = 
                    await testDbContext.UserCopyrights.
                    AsNoTracking().
                    CountAsync(
                        uc => uc.ApplicationUserId == userId);

                totalLinks.Should().Be(1);

                var link = 
                    await testDbContext.UserCopyrights.
                    AsNoTracking().
                    FirstAsync(
                        uc => uc.ApplicationUserId == userId && 
                        uc.CopyrightEntityId == entityId);

                link.IsDeleted.Should().
                    BeFalse();
            }
        }
    }
}
