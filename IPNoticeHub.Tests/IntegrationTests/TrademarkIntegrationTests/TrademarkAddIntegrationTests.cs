using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    [NonParallelizable]
    public class TrademarkAddIntegrationTests
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
        public async Task Post_AddValidTrademark_RedirectsToMyCollection_AndPersistsLink()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "ADD-MARK",
                    SourceId = "US-ADD-001",
                    RegistrationNumber = "RN-ADD-1",
                    GoodsAndServices = "Software; services",
                    Owner = "Acme Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;
            }

            var form = 
                new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString()
            };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            response.Headers.Location.Should().
                NotBeNull();

            var uriLocation = response.Headers.Location!;  
            
            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.
                    Set<UserTrademark>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == entityId);

                link.Should().
                    NotBeNull();

                link!.IsDeleted.Should().
                    BeFalse();
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithLocalReturnUrl_RedirectsToReturnUrl_AndPersistsLink()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;
            const string returnUrl = 
                "/Trademarks/MyCollection?page=2&sortBy=WordmarkAsc";

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(
                    testDbContext, 
                    userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "RETURN-ALPHA",
                    SourceId = "US-RET-001",
                    RegistrationNumber = "RN-RET-1",
                    GoodsAndServices = "Software; services",
                    Owner = "123 Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;
            }

            var form = 
                new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

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
                    Set<UserTrademark>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == entityId);

                link.Should().
                    NotBeNull();

                link!.IsDeleted.Should().
                    BeFalse();
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithExternalReturnUrl_IgnoresReturnUrl_AndPersistsLink()
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

                var entity = new TrademarkEntity
                {
                    Wordmark = "EXT-ALPHA",
                    SourceId = "US-EXT-001",
                    RegistrationNumber = "RN-EXT-1234567",
                    GoodsAndServices = "Software; services",
                    Owner = "123 Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;
            }

            var form = 
                new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = "https://example.com/away"
            };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            response.Headers.Location.Should().
                NotBeNull();

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.
                    Set<UserTrademark>().
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == entityId);

                link.Should().
                    NotBeNull();

                link!.IsDeleted.Should().
                    BeFalse();
            }
        }

        [Test]
        public async Task Post_Add_Unauthenticated_Returns401_AndNoLinkCreated()
        {
            var client = appFactory.CreateClient(
                new() { AllowAutoRedirect = false });

            int entityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var entity = new TrademarkEntity
                {
                    Wordmark = "UNAUTH-MARK",
                    SourceId = "US-UNAUTH-001",
                    RegistrationNumber = "RN-UNAUTH-1",
                    GoodsAndServices = "Software; services",
                    Owner = "Acme Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;
            }

            var form = 
                new Dictionary<string, string?> { ["trademarkId"] = 
                entityId.ToString() };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Unauthorized);

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var linkCount = await testDbContext.UserTrademarks.
                    AsNoTracking().
                    CountAsync(ut => ut.TrademarkEntityId == entityId);

                linkCount.Should().
                    Be(0);
            }
        }

        [Test]
        public async Task Post_AddTrademark_SoftDeletedLink_RestoresAndRedirectsToMyCollection()
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

                var entity = new TrademarkEntity
                {
                    Wordmark = "RESTORE-MARK",
                    SourceId = "US-RESTORE-001",
                    RegistrationNumber = "RN-RESTORE-1",
                    GoodsAndServices = "Software; services",
                    Owner = "Acme Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.
                    Set<UserTrademark>().
                    Add(new UserTrademark
                {
                    ApplicationUserId = userId,
                    TrademarkEntityId = entityId,
                    IsDeleted = true
                });

                await testDbContext.SaveChangesAsync();
            }

            var form = 
                new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString()
            };

            var response = await client.PostAsync(
                "/Trademarks/Add", new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var links = 
                    await testDbContext.UserTrademarks.
                    AsNoTracking().
                    Where(ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == entityId).
                    ToListAsync();

                links.Count.Should().
                    Be(1);

                links.Single().IsDeleted.Should().
                    BeFalse();
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithInvalidTrademarkId_RedirectsToMyCollection_AndNoLinkCreated()
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

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = "999999"
            };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var linkCount = await testDbContext.UserTrademarks.
                    AsNoTracking().
                    CountAsync();

                linkCount.Should().
                    Be(0);
            }
        }

        [Test]
        public async Task Post_AddTrademark_AuthenticatedMissingNameIdentifier_Returns403_AndNoLinkCreated()
        {
            int entityId;

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var entity = new TrademarkEntity
                {
                    Wordmark = "NoId WM",
                    SourceId = "US-NOIDWM-001",
                    RegistrationNumber = "RN-NOIDWM-1",
                    GoodsAndServices = "Software",
                    Owner = "ARW100",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;
            }

            var layeredFactory = 
                appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(o =>
                    {
                        o.DefaultAuthenticateScheme = "NoId";
                        o.DefaultChallengeScheme = "NoId";
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions, 
                        NoIdAuthHandler>(
                        "NoId", 
                        _ => { });
                });
            });

            await using (layeredFactory)
            {
                var client = layeredFactory.CreateClient(
                    new() { AllowAutoRedirect = false });

                var form = 
                    new Dictionary<string, string?> 
                    { ["trademarkId"] = entityId.ToString() };

                var response = await client.PostAsync(
                    "/Trademarks/Add", 
                    new FormUrlEncodedContent(form));

                response.StatusCode.Should().
                    Be(HttpStatusCode.Forbidden);

                using (var serviceScope = 
                    layeredFactory.Services.CreateScope())
                {
                    var testDbContext = 
                        serviceScope.ServiceProvider.
                        GetRequiredService<IPNoticeHubDbContext>();

                    var linkCount = 
                        await testDbContext.UserTrademarks.
                        AsNoTracking().
                        CountAsync();

                    linkCount.Should().
                        Be(0);
                }
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithMissingTrademarkId_RedirectsToMyCollection_NoLinkCreated()
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

            var response = await client.PostAsync(
                "/Trademarks/Add",
                new FormUrlEncodedContent(
                    new Dictionary<string, string?>()));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var linkCount = await testDbContext.UserTrademarks.
                    AsNoTracking().
                    CountAsync(ut => ut.ApplicationUserId == userId);

                linkCount.Should().Be(0);
            }
        }

        [TestCase("abc", TestName = "Post_Add_NonNumericId_Redirects_NoLinkCreated")]
        [TestCase("-1", TestName = "Post_Add_NegativeId_Redirects_NoLinkCreated")]
        public async Task Post_Add_InvalidTrademarkId_RedirectsToMyCollection_NoLinkCreated(string badId)
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

            var form = 
                new Dictionary<string, string?> { ["trademarkId"] = badId };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var linksCount = await testDbContext.UserTrademarks.
                    AsNoTracking().
                    CountAsync(ut => ut.ApplicationUserId == userId);

                linksCount.Should().
                    Be(0);
            }
        }

        [Test]
        public async Task Post_AddTrademark_AlreadyLinkedActive_DoesNotCreateDuplicate_RedirectsToMyCollection()
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

                var entity = new TrademarkEntity
                {
                    Wordmark = "No Duplicate WM",
                    SourceId = "US-NODUP-001",
                    RegistrationNumber = "RN-1234567-1",
                    GoodsAndServices = "Software",
                    Owner = "AZ1",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };
                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;

                testDbContext.UserTrademarks.Add(new UserTrademark
                {
                    ApplicationUserId = userId,
                    TrademarkEntityId = entityId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = 
                new Dictionary<string, string?> 
                { ["trademarkId"] = entityId.ToString() };

            var response = await client.PostAsync(
                "/Trademarks/Add", 
                new FormUrlEncodedContent(form));

            response.StatusCode.Should().
                Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;

            var resolvedUri = uriLocation.IsAbsoluteUri ? 
                uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().
                Be("/Trademarks/MyCollection");

            using (var serviceScope = 
                appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var links = await testDbContext.UserTrademarks.
                    AsNoTracking().
                    Where(ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == entityId).
                    ToListAsync();

                links.Count.Should().
                    Be(1);

                links[0].IsDeleted.Should().
                    BeFalse();
            }
        }

        private sealed class NoIdAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public NoIdAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder) : base(
                    options,
                    logger,
                    encoder)
            { }
            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "AuthNoId"),
                    new Claim(ClaimTypes.Email, "authnoid@test.local")
                }, "NoId");

                var principal = new ClaimsPrincipal(identity);

                var ticket = new AuthenticationTicket(
                    principal,
                    "NoId");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}
