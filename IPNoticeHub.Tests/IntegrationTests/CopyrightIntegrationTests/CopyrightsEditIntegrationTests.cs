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
    public class CopyrightsEditIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Get_EditCopyright_Linked_Returns200()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-200",
                    typeOfWork: "Literary",
                    title: "Edit Title");

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
                publicId = entity.PublicId;
            }

            var response = await client.GetAsync($"/Copyrights/Edit/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Post_EditCopyright_WithValidEnumWorkType_RedirectsToDetails_AndUpdatesEntity()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-302",
                    typeOfWork: "VisualArts",
                    title: "Initial Title");

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "New Title",
                ["Owner"] = "New Owner",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-12-31",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.AbsolutePath.Should().Be($"/Copyrights/Details/{publicId}");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var updatedEntity = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                      .FirstOrDefaultAsync(c => c.PublicId == publicId);

                updatedEntity.Should().NotBeNull();
                updatedEntity!.RegistrationNumber.Should().Be(regNumber);
                updatedEntity.Title.Should().Be("New Title");
                updatedEntity.Owner.Should().Be("New Owner");
                updatedEntity.TypeOfWork.Should().Be("Literary");
            }
        }

        [Test]
        public async Task Post_EditCopyright_WithWorkTypeOther_Redirects_AndStoresCustomType()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var copyrightEntity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-OTHER-302",
                    typeOfWork: "VisualArts",
                    title: "Old Title (Other)"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = copyrightEntity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = copyrightEntity.PublicId;
                regNumber = copyrightEntity.RegistrationNumber;
            }

            const string customType = "RandomType";

            var form = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Other",
                ["OtherWorkType"] = customType,
                ["Title"] = "New Title (Other)",
                ["Owner"] = "New Owner (Other)",
                ["YearOfCreation"] = "2023",
                ["DateOfPublication"] = "2023-06-30",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.AbsolutePath.Should().Be($"/Copyrights/Details/{publicId}");


            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var updatedEntity = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                                 .FirstOrDefaultAsync(c => c.PublicId == publicId);

                updatedEntity.Should().NotBeNull();
                updatedEntity!.RegistrationNumber.Should().Be(regNumber);
                updatedEntity.Title.Should().Be("New Title (Other)");
                updatedEntity.Owner.Should().Be("New Owner (Other)");
                updatedEntity.TypeOfWork.Should().Be(customType);
            }
        }

        [Test]
        public async Task Post_EditCopyright_WithMissingOtherWorkType_Returns200_AndDoesNotUpdate()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-INVALID-200",
                    typeOfWork: "VisualArts",
                    title: "Initial Title (Invalid Test)"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Other",
                ["OtherWorkType"] = "",                 // <-- missing, triggers ModelState error
                ["Title"] = "New Title (SHOULD-NOT-SAVE)",
                ["Owner"] = "New Owner (SHOULD-NOT-SAVE)",
                ["YearOfCreation"] = "2025",
                ["DateOfPublication"] = "2025-01-01",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var persisted = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                                   .FirstOrDefaultAsync(c => c.PublicId == publicId);

                persisted.Should().NotBeNull();
                persisted!.RegistrationNumber.Should().Be(regNumber);
                persisted.Title.Should().Be("Initial Title (Invalid Test)");
                persisted.Owner.Should().Be("Owner");
                persisted.TypeOfWork.Should().Be("VisualArts");
            }
        }

        [Test]
        public async Task Get_EditCopyright_WhenNotLinked_Returns404()
        {
            var targetUserId = "u1";
            var randomUserId = "u2";
            var client = appFactory.CreateClientAs(targetUserId);

            Guid publicId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, targetUserId);
                await TestDbSeeder.SeedUserAsync(testDbContext, randomUserId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-404",
                    typeOfWork: "Literary",
                    title: "Unlinked Edit Title");

                // Link ONLY the random user
                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
            }

            var response = await client.GetAsync($"/Copyrights/Edit/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Post_EditCopyright_WhenNotLinked_Returns200_AndDoesNotUpdate()
        {
            var targetUserId = "u1";
            var randomUserId = "u2";
            var client = appFactory.CreateClientAs(targetUserId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, targetUserId);
                await TestDbSeeder.SeedUserAsync(testDbContext, randomUserId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-NOTLINK-200",
                    typeOfWork: "VisualArts",
                    title: "Original Title"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = randomUserId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "SHOULD NOT APPLY",
                ["Owner"] = "SHOULD NOT APPLY",
                ["YearOfCreation"] = "2025",
                ["DateOfPublication"] = "2025-01-01",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            // Assert: controller re-renders the view (200) instead of redirecting
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var persisted = await testDbContext.CopyrightRegistrations
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(c => c.PublicId == publicId);

                persisted.Should().NotBeNull();
                persisted!.RegistrationNumber.Should().Be(regNumber);
                persisted.Title.Should().Be("Original Title");
                persisted.Owner.Should().Be("Owner");
                persisted.TypeOfWork.Should().Be("VisualArts");
            }
        }

        [Test]
        public async Task Get_EditCopyright_WithUnauthenticatedUser_Returns401()
        {
            // CreateClient() creates an anonymous client without authentication.
            var client = appFactory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            var randomId = Guid.NewGuid();

            var response = await client.GetAsync($"/Copyrights/Edit/{randomId}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Post_EditCopyright_WithUnauthenticatedUser_Returns401_AndDoesNotUpdate()
        {
            // CreateClient() creates an anonymous client without authentication.
            var client = appFactory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-UNAUTH-401",
                    typeOfWork: "VisualArts",
                    title: "Original Title (401)"
                );

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "SHOULD NOT SAVE",
                ["Owner"] = "SHOULD NOT SAVE",
                ["YearOfCreation"] = "2025",
                ["DateOfPublication"] = "2025-01-01",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var persisted = await testDbContext.CopyrightRegistrations
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(c => c.PublicId == publicId);

                persisted.Should().NotBeNull();
                persisted!.RegistrationNumber.Should().Be(regNumber);
                persisted.Title.Should().Be("Original Title (401)");
                persisted.Owner.Should().Be("Owner");          // from seeder
                persisted.TypeOfWork.Should().Be("VisualArts");
            }
        }

        [Test]
        public async Task Post_EditCopyright_WithValidLocalReturnUrl_RedirectsAndUpdatesEntity()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-RET-302",
                    typeOfWork: "VisualArts",
                    title: "Old Title (ReturnUrl)"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            const string returnUrl = "/Copyrights/MyCollection?page=2&sortBy=DateAddedDesc";

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "New Title (ReturnUrl)",
                ["Owner"] = "New Owner (ReturnUrl)",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-12-31",
                ["NationOfFirstPublication"] = "US",
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.PathAndQuery.Should().Be(returnUrl);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var updated = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                                 .FirstOrDefaultAsync(c => c.PublicId == publicId);

                updated.Should().NotBeNull();
                updated!.RegistrationNumber.Should().Be(regNumber);
                updated.Title.Should().Be("New Title (ReturnUrl)");
                updated.Owner.Should().Be("New Owner (ReturnUrl)");
                updated.TypeOfWork.Should().Be("Literary");
            }
        }

        [Test]
        public async Task Post_EditCopyright_WithValidWithExternalReturnUrl_IgnoresReturnUrl_AndRedirectsToDetails()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicId;
            string regNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-RET-EXT",
                    typeOfWork: "VisualArts",
                    title: "Old Title (ExtRet)"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = entity.Id,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
                regNumber = entity.RegistrationNumber;
            }

            const string externalReturnUrl = "https://example.com/phish";

            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicId.ToString(),
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "New Title (ExtReturnUrl)",
                ["Owner"] = "New Owner (ExtReturnUrl)",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-12-31",
                ["NationOfFirstPublication"] = "US",
                ["returnUrl"] = externalReturnUrl
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicId}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.AbsolutePath.Should().Be($"/Copyrights/Details/{publicId}");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var updated = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                                 .FirstOrDefaultAsync(c => c.PublicId == publicId);

                updated.Should().NotBeNull();
                updated!.RegistrationNumber.Should().Be(regNumber);
                updated.Title.Should().Be("New Title (ExtReturnUrl)");
                updated.Owner.Should().Be("New Owner (ExtReturnUrl)");
                updated.TypeOfWork.Should().Be("Literary");
            }
        }

        [Test]
        public async Task Post_EditCopyright_DuplicateRegistrationNumber_Returns200_AndDoesNotUpdate()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            Guid publicIdToEdit;
            int dbIdToEdit;
            string originalRegNumber;
            string duplicateRegNumber;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var originalEntity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-DUP-A",
                    typeOfWork: "VisualArts",
                    title: "Original Title (A)"
                );

                testDbContext.Set<UserCopyright>().Add(new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = originalEntity.Id,
                    IsDeleted = false
                });

                var duplicateEntity = await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-EDIT-DUP-B",
                    typeOfWork: "Literary",
                    title: "Some Other Title (B)"
                );

                await testDbContext.SaveChangesAsync();

                publicIdToEdit = originalEntity.PublicId;
                dbIdToEdit = originalEntity.Id;
                originalRegNumber = originalEntity.RegistrationNumber;
                duplicateRegNumber = duplicateEntity.RegistrationNumber;
            }

            // Post a form that tries to set the original entity's RegistrationNumber
            // to the duplicate entity's Registration number value
            var entityForm = new Dictionary<string, string?>
            {
                ["PublicId"] = publicIdToEdit.ToString(),
                ["RegistrationNumber"] = duplicateRegNumber,   // <-- attempt duplicate
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "New Title (SHOULD NOT APPLY)",
                ["Owner"] = "New Owner (SHOULD NOT APPLY)",
                ["YearOfCreation"] = "2025",
                ["DateOfPublication"] = "2025-01-01",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync($"/Copyrights/Edit/{publicIdToEdit}", new FormUrlEncodedContent(entityForm));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert DB: entity A unchanged
            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entityAAfter = await testDbContext.CopyrightRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dbIdToEdit);

                entityAAfter.Should().NotBeNull();
                entityAAfter!.RegistrationNumber.Should().Be(originalRegNumber);
                entityAAfter.Title.Should().Be("Original Title (A)");
                entityAAfter.Owner.Should().Be("Owner");
                entityAAfter.TypeOfWork.Should().Be("VisualArts");
            }
        }
    }
}
