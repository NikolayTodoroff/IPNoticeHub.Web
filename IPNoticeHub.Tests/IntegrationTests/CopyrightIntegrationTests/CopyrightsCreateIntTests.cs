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
    public class CopyrightsCreateIntTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Get_CreatePage_ReturnsHttpStatus200()
        {
            var client = appFactory.CreateClientAs("u1");
            var response = await client.GetAsync("/Copyrights/Create");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Post_Create_WithValidData_RedirectsToDetailsPage_AndPersistsEntityAndUserLink()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            var entityForm = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = "TX-9-123-456",
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "Test Title",
                ["Owner"] = "Test Owner",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-12-31",
                ["NationOfFirstPublication"] = "US"
            };
            var content = new FormUrlEncodedContent(entityForm!);

            var response = await client.PostAsync("/Copyrights/Create", content);

            // Assert HTTP redirect
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            // Handle relative Location header safely
            var location = response.Headers.Location!;
            var effectiveUri = location.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);

            effectiveUri.AbsolutePath.Should().StartWith("/Copyrights/Details");

            // Extracting the GUID Id
            var idSegment = effectiveUri.Segments[^1].TrimEnd('/');
            Guid.TryParse(idSegment, out var publicId).Should().BeTrue("Details redirect must include a Guid id");

            // Assert DB: entity + user link
            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await testDbContext.CopyrightRegistrations.AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.PublicId == publicId);

                entity.Should().NotBeNull();
                entity!.RegistrationNumber.Should().Be("TX-9-123-456");
                entity.Title.Should().Be("Test Title");
                entity.Owner.Should().Be("Test Owner");

                bool anyExistingLinks = await testDbContext.Set<UserCopyright>().AnyAsync(uc => uc.ApplicationUserId == userId
                                                      && uc.CopyrightRegistrationId == entity.Id
                                                      && !uc.IsDeleted);

                anyExistingLinks.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Create_WithInvalidModel_ReturnsHttpStatus200_AndDoesNotPersistEntity()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            const string regNumber = "TX-9-INVALID-000";
            var form = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "",                // invalid
                ["Owner"] = "Test Owner",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-12-31",
                ["NationOfFirstPublication"] = "US"
            };
            var content = new FormUrlEncodedContent(form!);

            var response = await client.PostAsync("/Copyrights/Create", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                bool anyWithReg = await testDbContext.CopyrightRegistrations.AsNoTracking().AnyAsync(c => c.RegistrationNumber == regNumber);

                anyWithReg.Should().BeFalse("invalid model must not create a registration");

                bool anyExistingLinks = await testDbContext.Set<UserCopyright>().AnyAsync(uc => uc.ApplicationUserId == userId);

                anyExistingLinks.Should().BeFalse("Invalid model must not create a user–copyright link");
            }
        }

        [Test]
        public async Task Post_Create_WithCustomWorkType_Redirects_AndPersistsCustomType()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            const string customType = "Software";
            const string regNumber = "TX-9-OTHER-777";

            var entityForm = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Other",
                ["OtherWorkType"] = customType,
                ["Title"] = "Other Work Title",
                ["Owner"] = "Other Owner",
                ["YearOfCreation"] = "2023",
                ["DateOfPublication"] = "2023-06-30",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync("/Copyrights/Create", new FormUrlEncodedContent(entityForm!));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var effectiveUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            effectiveUri.AbsolutePath.Should().StartWith("/Copyrights/Details");

            var idSegment = effectiveUri.Segments[^1].TrimEnd('/');
            Guid.TryParse(idSegment, out var publicId).Should().BeTrue();

            using (var scope = appFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await db.CopyrightRegistrations.AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.PublicId == publicId);

                entity.Should().NotBeNull();
                entity!.RegistrationNumber.Should().Be(regNumber);
                entity.Title.Should().Be("Other Work Title");
                entity.Owner.Should().Be("Other Owner");

                // Verifying TypeOfWork is the raw OtherWorkType text (NormalizeWorkType logic)
                entity.TypeOfWork.Should().Be(customType);

                var linkExists = await db.Set<UserCopyright>().AnyAsync(uc => uc.ApplicationUserId == userId
                                                      && uc.CopyrightRegistrationId == entity.Id
                                                      && !uc.IsDeleted);

                linkExists.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Create_Valid_WithLocalReturnUrl_RedirectsToReturnUrl_AndPersists()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            const string regNo = "TX-9-RET-123";
            const string returnUrl = "/Copyrights/MyCollection?page=1&sortBy=DateAdded";

            var form = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = regNo,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "ReturnUrl Title",
                ["Owner"] = "ReturnUrl Owner",
                ["YearOfCreation"] = "2022",
                ["DateOfPublication"] = "2022-05-05",
                ["NationOfFirstPublication"] = "US",
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync("/Copyrights/Create", new FormUrlEncodedContent(form!));

            // Assert redirect to the provided local URL
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var effectiveUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            effectiveUri.PathAndQuery.Should().Be(returnUrl);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await testDbContext.CopyrightRegistrations.AsNoTracking().
                                     FirstOrDefaultAsync(c => c.RegistrationNumber == regNo);

                entity.Should().NotBeNull();
                entity!.Title.Should().Be("ReturnUrl Title");
                entity.Owner.Should().Be("ReturnUrl Owner");

                var linkExists = await testDbContext.Set<UserCopyright>().AnyAsync(uc => uc.ApplicationUserId == userId
                                                      && uc.CopyrightRegistrationId == entity.Id
                                                      && !uc.IsDeleted);

                linkExists.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Create_Valid_WithExternalReturnUrl_IgnoresReturnUrl_AndRedirectsToDetails()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);
            }

            const string regNumber = "TX-9-RET-EXT-001";
            const string externalReturnUrl = "https://example.com/evil";

            var entityForm = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "External ReturnUrl Title",
                ["Owner"] = "External ReturnUrl Owner",
                ["YearOfCreation"] = "2021",
                ["DateOfPublication"] = "2021-01-15",
                ["NationOfFirstPublication"] = "US",
                ["returnUrl"] = externalReturnUrl
            };

            var response = await client.PostAsync("/Copyrights/Create", new FormUrlEncodedContent(entityForm!));

            // Assert: should redirect to Details (not to the external URL)
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var effectiveUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            effectiveUri.AbsolutePath.Should().StartWith("/Copyrights/Details");

            var idSegment = effectiveUri.Segments[^1].TrimEnd('/');
            Guid.TryParse(idSegment, out var publicId).Should().BeTrue();

            using (var scope = appFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = await db.CopyrightRegistrations
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.PublicId == publicId);

                entity.Should().NotBeNull();
                entity!.RegistrationNumber.Should().Be(regNumber);

                entity.Title.Should().Be("External ReturnUrl Title");
                entity.Owner.Should().Be("External ReturnUrl Owner");

                var linkExists = await db.Set<UserCopyright>().AnyAsync(uc => uc.ApplicationUserId == userId
                                                      && uc.CopyrightRegistrationId == entity.Id
                                                      && !uc.IsDeleted);

                linkExists.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Create_Unauthenticated_ReturnsUnauthorizedStatus_AndDoesNotPersistEntity()
        {
            // No user is authenticated, so do not call CreateClientAs(...) to simulate an unauthenticated request.
            var client = appFactory.CreateClient();

            const string regNumber = "TX-9-UNAUTH-001";
            var entityForm = new Dictionary<string, string?>
            {
                ["RegistrationNumber"] = regNumber,
                ["WorkType"] = "Literary",
                ["OtherWorkType"] = "",
                ["Title"] = "Should Not Save",
                ["Owner"] = "Nobody",
                ["YearOfCreation"] = "2024",
                ["DateOfPublication"] = "2024-01-01",
                ["NationOfFirstPublication"] = "US"
            };

            var response = await client.PostAsync("/Copyrights/Create", new FormUrlEncodedContent(entityForm!));

            // TestAuth only authenticates when a user ID is explicitly set; without it, the HasUserId policy denies access with a 401 status.
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Assert nothing persisted
            using var serviceScope = appFactory.Services.CreateScope();
            var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            bool entityExists = await testDbContext.CopyrightRegistrations.AsNoTracking()
                .AnyAsync(c => c.RegistrationNumber == regNumber);

            entityExists.Should().BeFalse("Unauthenticated users are not allowed not create registrations");
        }   
    }
}
