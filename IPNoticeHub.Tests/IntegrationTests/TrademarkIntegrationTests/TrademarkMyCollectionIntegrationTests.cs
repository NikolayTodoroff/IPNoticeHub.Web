using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    [NonParallelizable]
    public class TrademarkMyCollectionIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp()
        {
            appFactory = new TestWebAppFactory();
        }

        [TearDown]
        public void TearDown()
        {
            appFactory.Dispose();
        }

        [Test]
        public async Task Get_MyCollection_Unauthenticated_Returns401()
        {
            var client = appFactory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync(
                "/Trademarks/MyCollection");

            response.StatusCode.Should().
                Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Get_MyCollection_AuthenticatedWithoutNameIdentifier_Returns403()
        {
            var layeredFactory = 
                appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication(authOptions =>
                        {
                            authOptions.DefaultAuthenticateScheme = "NoId";
                            authOptions.DefaultChallengeScheme = "NoId";
                        })
                        .AddScheme<
                            AuthenticationSchemeOptions, 
                            NoIdAuthHandler>("NoId", 
                            _ => { });
                });
            });

            await using (layeredFactory)
            {
                var client = layeredFactory.CreateClient(
                    new() 
                    { AllowAutoRedirect = false });

                var response = await client.GetAsync(
                    "/Trademarks/MyCollection");

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
                        "authnoid@test.local")
                }, "NoId");

                var claimsPrincipal = new ClaimsPrincipal(identity);

                var authTicket = new AuthenticationTicket(
                    claimsPrincipal, 
                    "NoId");

                return Task.FromResult(AuthenticateResult.Success(authTicket));
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

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "First Entity",
                    SourceId = "US-PAGE-A",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "Second Entity",
                    SourceId = "US-PAGE-B",
                    RegistrationNumber = "RN-B",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                var entity3 = new TrademarkEntity
                {
                    Wordmark = "Third Entity",
                    SourceId = "US-PAGE-C",
                    RegistrationNumber = "RN-C",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2, 
                    entity3);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity3.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=2&resultsPerPage=1");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithOutOfRangePaging_Returns200()
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

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "First WM",
                    SourceId = "US-OUTR-1",
                    RegistrationNumber = "RN-1",
                    GoodsAndServices = "Software",
                    Owner = "Owner1",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };
                var entity2 = new TrademarkEntity
                {
                    Wordmark = "Second WM",
                    SourceId = "US-OUTR-2",
                    RegistrationNumber = "RN-2",
                    GoodsAndServices = "Software",
                    Owner = "Owner2",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=9999&resultsPerPage=9999");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithInvalidSortParameter_Returns200()
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

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "WM1",
                    SourceId = "US-SORT-1",
                    RegistrationNumber = "RN-1",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "WM2",
                    SourceId = "US-SORT-2",
                    RegistrationNumber = "RN-2",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=TotallyNotAValue&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
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

                var activeEntity = new TrademarkEntity
                {
                    Wordmark = "ACTIVE-MARK",
                    SourceId = "US-SDEL-A",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                var deletedEntity = new TrademarkEntity
                {
                    Wordmark = "DELETED-MARK",
                    SourceId = "US-SDEL-D",
                    RegistrationNumber = "RN-D",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    activeEntity, 
                    deletedEntity);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = activeEntity.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = deletedEntity.Id, 
                        IsDeleted = true }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WhenCollectionIsEmpty_Returns200()
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
                "/Trademarks/MyCollection");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public async Task Get_MyCollection_WhenResultsPerPage_NonPositive_Returns200(int resultsPerPage)
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

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "A1",
                    SourceId = "US-1111",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme", 
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "B2",
                    SourceId = "US-2222",
                    RegistrationNumber = "RN-B",
                    GoodsAndServices = "Software",
                    Owner = "Acme", 
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=1&resultsPerPage=" +
                $"{resultsPerPage}");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(5000)]
        [TestCase(10000)]
        public async Task Get_MyCollection_WhenResultsPerPage_VeryLarge_Returns200(int resultsPerPage)
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

                var entity1 = new TrademarkEntity
                { 
                    Wordmark = "A1",
                    SourceId = "US-1111",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "B2",
                    SourceId = "US-2222",
                    RegistrationNumber = "RN-B",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                var entity3 = new TrademarkEntity
                {
                    Wordmark = "C3",
                    SourceId = "US-3333",
                    RegistrationNumber = "RN-C",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2, 
                    entity3);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity3.Id, 
                        IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=1&resultsPerPage=" +
                $"{resultsPerPage}");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [TestCase(0)]
        [TestCase(-3)]
        public async Task Get_MyCollection_WhenCurrentPage_NonPositive_Returns200(int currentPage)
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

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "Zebrea1000",
                    SourceId = "US-1111-A",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "DADA200",
                    SourceId = "US-2222-B",
                    RegistrationNumber = "RN-B",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=" +
                $"{currentPage}&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WhenCurrentPage_NonNumeric_Returns200()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "ALPHA",
                    SourceId = "US-CPSTR-A",
                    RegistrationNumber = "RN-A",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "BRAVO",
                    SourceId = "US-CPSTR-B",
                    RegistrationNumber = "RN-B",
                    GoodsAndServices = "Software",
                    Owner = "Acme",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO };

                testDbContext.TrademarkRegistrations.AddRange(
                    entity1, 
                    entity2);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity1.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = entity2.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=abc&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_TrademarksSearch_Path_Returns404()
        {
            var client = appFactory.CreateClient(
                new() 
                { AllowAutoRedirect = false });

            var response = await client.GetAsync(
                "/Trademarks/Search");

            response.StatusCode.Should().
                Be(HttpStatusCode.NotFound);
        }

        [TestCase("RegistrationDateAsc")]
        [TestCase("RegistrationDateDesc")]
        public async Task Get_MyCollection_WhenSortBy_RegistrationDateVariants_Returns200(string sortBy)
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

                var initialEntity = new TrademarkEntity
                {
                    Wordmark = "Initial WM",
                    SourceId = "US-SORT-RD-1",
                    RegistrationNumber = "RN-Initial1",
                    GoodsAndServices = "Software",
                    Owner = "AZ1",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    RegistrationDate = new DateTime(2020, 1, 1),
                    Source = DataProvider.USPTO
                };

                var newEntity = new TrademarkEntity
                {
                    Wordmark = "New WM",
                    SourceId = "US-SORT-RD-2",
                    RegistrationNumber = "RN-New1",
                    GoodsAndServices = "Software",
                    Owner = "AZ1",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    RegistrationDate = new DateTime(2022, 6, 15),
                    Source = DataProvider.USPTO
                };

                var noRegDateEntity = new TrademarkEntity
                {
                    Wordmark = "No RegDate WM",
                    SourceId = "US-SORT-RD-3",
                    RegistrationNumber = "RN-ND",
                    GoodsAndServices = "Software",
                    Owner = "AZ1",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    RegistrationDate = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(
                    initialEntity, 
                    newEntity, 
                    noRegDateEntity);

                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = initialEntity.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = newEntity.Id, 
                        IsDeleted = false },

                    new UserTrademark { 
                        ApplicationUserId = userId, 
                        TrademarkEntityId = noRegDateEntity.Id, 
                        IsDeleted = false }
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                $"/Trademarks/MyCollection?sortBy=" +
                $"{sortBy}&currentPage=1&resultsPerPage=10");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_MyCollection_WithLargeDataset_Paging_Returns200()
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

                var list = new List<TrademarkEntity>(120);

                for (int i = 0; i < 120; i++)
                {
                    list.Add(new TrademarkEntity
                    {
                        Wordmark = $"BULK-{i:D3}",
                        SourceId = $"US-BULK-{i:D5}",
                        RegistrationNumber = $"RN-{i:D5}",
                        GoodsAndServices = "Software",
                        Owner = "Bulk Inc.",
                        StatusCategory = TrademarkStatusCategory.Registered,
                        StatusDetail = "Registered",
                        Source = DataProvider.USPTO
                    });
                }

                testDbContext.TrademarkRegistrations.AddRange(list);
                await testDbContext.SaveChangesAsync();

                testDbContext.UserTrademarks.AddRange(
                    list.Select(
                        t => new UserTrademark
                    {
                        ApplicationUserId = userId,
                        TrademarkEntityId = t.Id,
                        IsDeleted = false
                    })
                );
                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync(
                "/Trademarks/MyCollection?sortBy=WordmarkAsc&currentPage=2&resultsPerPage=50");

            response.StatusCode.Should().
                Be(HttpStatusCode.OK);
        }
    }
}


