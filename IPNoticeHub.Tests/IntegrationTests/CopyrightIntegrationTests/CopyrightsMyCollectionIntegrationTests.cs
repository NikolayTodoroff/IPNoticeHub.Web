using FluentAssertions;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Domain.Entities.Copyrights;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests
{
    [NonParallelizable]
    public class CopyrightsMyCollectionIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Get_MyCollection_LinkedItems_Returns200()
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

                var copyrightEntity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-001",
                    typeOfWork: "Literary", 
                    title: "A Title");

                var copyrightEntity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-002",
                    typeOfWork: "VisualArts", 
                    title: "B Title");

                testDbContext.Set<UserCopyright>().AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = copyrightEntity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = copyrightEntity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_AuthenticatedMissingNameIdentifier_Returns403()
        {
            var layeredAppFactory = appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "NoId";
                        options.DefaultChallengeScheme = "NoId";
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions, 
                        NoIdAuthHandler>(
                        "NoId",
                        _ => { });
                });

                builder.UseSetting(
                    "Authentication:DefaultScheme", 
                    "NoId");
            });

            await using (layeredAppFactory) 
            {
                var client = layeredAppFactory.CreateClient(
                    new() { AllowAutoRedirect = false });

                var response = await client.GetAsync(
                    "/Copyrights/MyCollection");

                response.StatusCode.Should().
                    Be(HttpStatusCode.Forbidden);
            }
        }

        private sealed class NoIdAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public NoIdAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, 
                UrlEncoder encoder) : base(options, logger, encoder) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var identity = new ClaimsIdentity(new[]
                {
                new Claim(
                    ClaimTypes.Name, 
                    "AuthNoId"),

                new Claim(
                    ClaimTypes.Email, 
                    "authnoid@test")
                
            }, "NoId");

                var principal = new ClaimsPrincipal(identity);

                var ticket = new AuthenticationTicket(
                    principal, 
                    "NoId");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }

        [Test]
        public async Task Get_MyCollection_WithPagingAndSorting_Returns200()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-PAGE-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-PAGE-B",
                    typeOfWork: "Literary", 
                    title: "Bravo");

                var entity3 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-COLL-PAGE-C",
                    typeOfWork: "Literary", 
                    title: "Charlie");

                testDbContext.Set<UserCopyright>().AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity3.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=2&resultsPerPage=1");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithOutOfRangePagingParameters_Returns200()
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-ROBUST-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-ROBUST-B",
                    typeOfWork: "Literary", 
                    title: "Bravo");

                testDbContext.Set<UserCopyright>().AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=9999&resultsPerPage=9999");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_InvalidSortBy_Returns200()
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-SORT-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-SORT-B",
                    typeOfWork: "VisualArts", 
                    title: "Bravo");

                testDbContext.
                    Set<UserCopyright>().
                    AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TotallyNotAValue&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_IgnoresSoftDeletedLinks_Returns200()
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

                var activeEntity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-ACTIVE",
                    typeOfWork: "Literary", 
                    title: "Active");

                var deletedEntity = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-DELETED",
                    typeOfWork: "VisualArts", 
                    title: "Deleted");

                testDbContext.
                    Set<UserCopyright>().
                    AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = activeEntity.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = deletedEntity.Id, 
                        IsDeleted = true }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithEmptyCollection_Returns200()
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

            var response = await client.GetAsync(
                "/Copyrights/MyCollection");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public async Task Get_MyCollection_ResultsPerPage_NonPositive_Returns200(int resultsPerPage)
        {
            // Arrange
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-RPP-A", 
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-RPP-B", 
                    typeOfWork: "VisualArts", 
                    title: "Bravo");

                testDbContext.
                    Set<UserCopyright>().
                    AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var url = 
                $"/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=1&resultsPerPage=" +
                $"{resultsPerPage}";

            var response = await client.GetAsync(url);

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(5000)]
        [TestCase(10000)]
        public async Task Get_MyCollection_ResultsPerPage_VeryLarge_Returns200(int resultsPerPage)
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-RPP-LARGE-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-RPP-LARGE-B",
                    typeOfWork: "VisualArts", 
                    title: "Bravo");

                var entity3 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-RPP-LARGE-C",
                    typeOfWork: "Literary", 
                    title: "Charlie");

                testDbContext.Set<UserCopyright>().AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity3.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var url = 
                $"/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=1&resultsPerPage=" +
                $"{resultsPerPage}";

            var response = await client.GetAsync(url);

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(0)]
        [TestCase(-3)]
        public async Task Get_MyCollection_CurrentPage_NonPositive_Returns200(int currentPage)
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-COLL-PAGE-NEG-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-PAGE-NEG-B",
                    typeOfWork: "VisualArts", 
                    title: "Bravo");

                testDbContext.
                    Set<UserCopyright>().
                    AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=" +
                $"{currentPage}&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithCurrentPage_NonNumeric_Returns200()
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

                var entity1 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext,
                    regNumber: "TX-9-COLL-PAGE-A",
                    typeOfWork: "Literary", 
                    title: "Alpha");

                var entity2 = 
                    await TestDbSeeder.SeedCopyrightAsync(
                    testDbContext, 
                    regNumber: "TX-9-COLL-PAGE-B",
                    typeOfWork: "VisualArts", 
                    title: "Bravo");

                testDbContext.Set<UserCopyright>().AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=abc&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase("TitleAsc")]
        [TestCase("TitleDesc")]
        public async Task Get_MyCollection_SortBy_Title_Variants_Return200(string sortBy)
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

                var entity1 = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA-CP-SORT-001",
                    TypeOfWork = "Literary Work",
                    Title = "AAA",
                    YearOfCreation = 2021,
                    Owner = "AZ LLC"
                };
                var entity2 = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA-CP-SORT-002",
                    TypeOfWork = "Literary Work",
                    Title = "BBB",
                    YearOfCreation = 2022,
                    Owner = "AZ LLC"
                };
                var entity3 = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA-CP-SORT-003",
                    TypeOfWork = "Literary Work",
                    Title = "CCC",
                    YearOfCreation = 2023,
                    Owner = "AZ LLC"
                };

                testDbContext.CopyrightRegistrations.AddRange(
                    entity1, 
                    entity2, 
                    entity3);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserCopyrights.AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false, 
                        DateAdded = DateTime.UtcNow.AddDays(-2) },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false, 
                        DateAdded = DateTime.UtcNow.AddDays(-1) },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity3.Id, 
                        IsDeleted = false, 
                        DateAdded = DateTime.UtcNow }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Copyrights/MyCollection?sortBy=" +
                $"{sortBy}&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_CurrentPage_NonNumeric_Returns200()
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

                var entity1 = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA-CP-PAGE-001",
                    TypeOfWork = "Literary Work",
                    Title = "AAA",
                    YearOfCreation = 2022,
                    Owner = "AZ LLC"
                };
                var entity2 = new CopyrightEntity
                {
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "VA-CP-PAGE-002",
                    TypeOfWork = "Literary Work",
                    Title = "BBB",
                    YearOfCreation = 2023,
                    Owner = "AZ LLC"
                };

                testDbContext.CopyrightRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserCopyrights.AddRange(
                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity1.Id, 
                        IsDeleted = false, 
                        DateAdded = DateTime.UtcNow.AddDays(-1) },

                    new UserCopyright { 
                        ApplicationUserId = userId, 
                        CopyrightEntityId = entity2.Id, 
                        IsDeleted = false, 
                        DateAdded = DateTime.UtcNow }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=abc&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_LargeDataset_Paging_Returns200()
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

                var list = 
                    new List<CopyrightEntity>(120);

                for (int i = 0; i < 120; i++)
                {
                    list.Add(new CopyrightEntity
                    {
                        PublicId = Guid.NewGuid(),
                        RegistrationNumber = $"VA-BULK-{i:D6}",
                        TypeOfWork = "Literary Work",
                        Title = $"BULK-{i:D3}",
                        YearOfCreation = 2020 + (i % 5),
                        Owner = "Bulk Inc."
                    });
                }

                testDbContext.CopyrightRegistrations.AddRange(list);
                await testDbContext.SaveChangesAsync();

                testDbContext.UserCopyrights.AddRange(
                    list.Select(
                        uc => new UserCopyright
                    {
                        ApplicationUserId = userId,
                        CopyrightEntityId = uc.Id,
                        IsDeleted = false,
                        DateAdded = DateTime.UtcNow.AddMinutes(-uc.Id % 60)
                    })
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Copyrights/MyCollection?sortBy=TitleAsc&currentPage=2&resultsPerPage=50");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_Details_WithInvalidRouteToken_Returns404()
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

            var response = await client.GetAsync(
                "/Copyrights/Details/not-a-guid");

            response.StatusCode.Should().
                Be(HttpStatusCode.NotFound);
        }
    }
}
