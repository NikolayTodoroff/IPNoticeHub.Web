using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    public class TrademarkRemoveIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

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
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = entityId,
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
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

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
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = entityId,
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
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

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
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = entityId,
                    IsDeleted = false
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = "https://evil.example/away" // must be ignored
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
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

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
                    .CountAsync(ut => ut.TrademarkRegistrationId == entityId);

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
                    ApplicationUserId = randomUserId,
                    TrademarkRegistrationId = entityId,
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
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == targetUserId && ut.TrademarkRegistrationId == entityId);

                callerLink.Should().BeNull("caller wasn't linked; nothing to delete");

                var otherLink = await testDbContext.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == randomUserId && ut.TrademarkRegistrationId == entityId);

                otherLink.Should().NotBeNull();
                otherLink!.IsDeleted.Should().BeFalse();
            }
        }
    }
}
