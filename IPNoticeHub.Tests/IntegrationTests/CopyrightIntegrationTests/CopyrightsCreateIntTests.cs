using System.Net;
using System.Net.Http;
using System.Collections.Generic;
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

                bool anyExistingLinks = await testDbContext.Set<UserCopyright>()
                                         .AnyAsync(uc => uc.ApplicationUserId == userId
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

                anyExistingLinks.Should().BeFalse("invalid model must not create a user–copyright link");
            }
        }
    }
}
