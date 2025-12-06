using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
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

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    public class TrademarkRemoveIntegrationTests
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
        public async Task Post_RemoveTrademark_Linked_RedirectsToMyCollection_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "Power Rangers",
                    SourceId = "US-Source001",
                    RegistrationNumber = "RN-111-12345",
                    GoodsAndServices = "Software; services",
                    Owner = "Deadfire LTD",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.PostAsync("/Trademarks/Remove",
                new FormUrlEncodedContent(new Dictionary<string, string?> { ["trademarkId"] = entityId.ToString() }));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveTrademark_WithLocalReturnUrl_RedirectsToReturnUrl_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;
            const string returnUrl = "/Trademarks/MyCollection?page=2&sortBy=WordmarkDesc";

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "Down With The Sun",
                    SourceId = "US-1234567",
                    RegistrationNumber = "RN-1-123456",
                    GoodsAndServices = "Software; services",
                    Owner = "Moon and Stars LLC",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.PathAndQuery.Should().Be(returnUrl);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_RemoveTrademark_WithExternalReturnUrl_IgnoresReturnUrl_AndSoftDeletes()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "Static 100",
                    SourceId = "US-REM-EXT-001",
                    RegistrationNumber = "RN-REM-EXT-1",
                    GoodsAndServices = "Software; services",
                    Owner = "ArmorZ Inc.",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = "https://evil.example/away"
            };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Remove_Unauthenticated_Returns401_AndNoChanges()
        {
            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = new TrademarkEntity
                {
                    Wordmark = "Target Z1",
                    SourceId = "US-UNAUTH-111111",
                    RegistrationNumber = "RN-UNAUTH-123446",
                    GoodsAndServices = "Software; services",
                    Owner = "AZXL Inc.",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;
            }

            var form = new Dictionary<string, string?> { ["trademarkId"] = entityId.ToString() };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var linkCount = await testDbContext.UserTrademarks.AsNoTracking()
                    .CountAsync(ut => ut.TrademarkId == entityId);

                linkCount.Should().Be(0);
            }
        }

        [Test]
        public async Task Post_RemoveTrademark_NotLinked_RedirectsToMyCollection_AndMakesNoChanges()
        {
            var targetUserId = "u1";
            var randomUserId = "u2";
            var client = appFactory.CreateClientAs(targetUserId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, targetUserId);
                await TestDbSeeder.SeedUserAsync(testDbContext, randomUserId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "Not Linked WM",
                    SourceId = "US-S11111",
                    RegistrationNumber = "RN-S1-10000",
                    GoodsAndServices = "Software; services",
                    Owner = "A1Z2 LLC",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = randomUserId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString()
            };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

           resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var callerLink = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.UserId == targetUserId && ut.TrademarkId == entityId);

                callerLink.Should().BeNull("caller wasn't linked; nothing to delete");

                var otherLink = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.UserId == randomUserId && ut.TrademarkId == entityId);

                otherLink.Should().NotBeNull();
                otherLink!.IsDeleted.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_Remove_WithNonExistingTrademark_RedirectsToMyCollection_NoChange()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);
            int existingTmId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "Existing WM",
                    SourceId = "US-Existing-001",
                    RegistrationNumber = "RN-EX-1",
                    GoodsAndServices = "Software",
                    Owner = "AZ100 LLC",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                existingTmId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = existingTmId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?> { ["trademarkId"] = "999999" };
            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var linksCount = await testDbContext.UserTrademarks.AsNoTracking()
                    .CountAsync(ut => ut.UserId == userId);

                linksCount.Should().Be(1);

                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstAsync(ut => ut.UserId == userId && ut.TrademarkId == existingTmId);

                link.IsDeleted.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_RemoveTrademark_AlreadySoftDeleted_RedirectsToMyCollection_WithoutChanges()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "WM100",
                    SourceId = "US-IDEM-001",
                    RegistrationNumber = "RN-IDEM-1",
                    GoodsAndServices = "Software; services",
                    Owner = "ARD Inc.",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);

                await testDbContext.SaveChangesAsync();

                entityId = entity.Id;

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = true
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?> { ["trademarkId"] = entityId.ToString() };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var linksCount = await testDbContext.UserTrademarks.AsNoTracking()
                    .CountAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                linksCount.Should().Be(1);

                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.IsDeleted.Should().BeTrue();
            }
        }

        [Test]
        public async Task Post_Remove_AuthenticatedMissingNameIdentifier_Returns403_AndNoChange()
        {
            var layeredFactory = appFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(authOptions =>
                    {
                        authOptions.DefaultAuthenticateScheme = "NoId";
                        authOptions.DefaultChallengeScheme = "NoId";
                    })
                    .AddScheme<AuthenticationSchemeOptions, NoIdAuthHandler>("NoId", _ => { });
                });
            });

            await using (layeredFactory)
            {
                int entityId;
                const string someUserId = "uExisting";

                using (var serviceScope = layeredFactory.Services.CreateScope())
                {
                    var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                    await TestDbSeeder.SeedUserAsync(testDbContext, someUserId);

                    var entity = new TrademarkEntity
                    {
                        Wordmark = "NOID-REMOVE",
                        SourceId = "US-NOID-REM-001",
                        RegistrationNumber = "RN-NOID-REM-1",
                        GoodsAndServices = "Software",
                        Owner = "Acme",
                        StatusCategory = TrademarkStatusCategory.Registered,
                        StatusDetail = "Registered",
                        Source = DataProvider.USPTO
                    };
                    testDbContext.TrademarkRegistrations.Add(entity);
                    await testDbContext.SaveChangesAsync();
                    entityId = entity.Id;

                    testDbContext.UserTrademarks.Add(new UserTrademark
                    {
                        UserId = someUserId,
                        TrademarkId = entityId,
                        IsDeleted = false
                    });
                    await testDbContext.SaveChangesAsync();
                }

                var client = layeredFactory.CreateClient(new() { AllowAutoRedirect = false });

                var form = new Dictionary<string, string?> { ["trademarkId"] = entityId.ToString() };
                var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

                using (var serviceScope = layeredFactory.Services.CreateScope())
                {
                    var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                    var link = await testDbContext.UserTrademarks.AsNoTracking()
                        .FirstOrDefaultAsync(ut => ut.UserId == someUserId && ut.TrademarkId == entityId);

                    link.Should().NotBeNull();
                    link!.IsDeleted.Should().BeFalse();
                }
            }
        }

        private sealed class NoIdAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public NoIdAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                   ILoggerFactory logger,UrlEncoder encoder) : base(options, logger, encoder) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, "AuthNoId"),
                new Claim(ClaimTypes.Email, "authnoid@test.local")
            }, "NoId");

                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "NoId");
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }

        [Test]
        public async Task Post_Remove_WhenTrademarkIdIsMissing_RedirectsToMyCollection_NoChanges()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "WM1",
                    SourceId = "US-Z1235-001",
                    RegistrationNumber = "RN-1111-1",
                    GoodsAndServices = "Software",
                    Owner = "AMA LLC",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);

                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;

                testDbContext.UserTrademarks.Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.PostAsync("/Trademarks/Remove",
                new FormUrlEncodedContent(new Dictionary<string, string?>()));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var resolvedUri = (response.Headers.Location!.IsAbsoluteUri) ?
                 response.Headers.Location! : new Uri(client.BaseAddress!, response.Headers.Location!);

                 resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.IsDeleted.Should().BeFalse();
            }
        }

        [TestCase("abc", TestName = "Post_Remove_NonNumericId_Redirects_NoChange")]
        [TestCase("-1", TestName = "Post_Remove_NegativeId_Redirects_NoChange")]
        public async Task Post_Remove_InvalidTrademarkId_RedirectsToMyCollection_NoChange(string badId)
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity = new TrademarkEntity
                {
                    Wordmark = "WM1",
                    SourceId = "US-Wm1-001",
                    RegistrationNumber = "RN-WM12345-2",
                    GoodsAndServices = "Software",
                    Owner = "AZAZ101",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                entityId = entity.Id;

                testDbContext.UserTrademarks.Add(new UserTrademark
                {
                    UserId = userId,
                    TrademarkId = entityId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?> { ["trademarkId"] = badId };

            var response = await client.PostAsync("/Trademarks/Remove", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var resolvedUri = (response.Headers.Location!.IsAbsoluteUri) ?
                response.Headers.Location! : new Uri(client.BaseAddress!, response.Headers.Location!);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");


            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstAsync(ut => ut.UserId == userId && ut.TrademarkId == entityId);

                link.IsDeleted.Should().BeFalse();
            }
        }
    }
}

