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
    public class TrademarkAddIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Post_AddValidTrademark_RedirectsToMyCollection_AndPersistsLink()
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

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString()
            };

            var response = await client.PostAsync("/Trademarks/Add", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.Set<UserTrademark>().AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithLocalReturnUrl_RedirectsToReturnUrl_AndPersistsLink()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            int entityId;
            const string returnUrl = "/Trademarks/MyCollection?page=2&sortBy=WordmarkAsc";

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

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

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = returnUrl
            };

            var response = await client.PostAsync("/Trademarks/Add", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            resolvedUri.PathAndQuery.Should().Be(returnUrl);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.Set<UserTrademark>().AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_AddTrademark_WithExternalReturnUrl_IgnoresReturnUrl_AndPersistsLink()
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

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString(),
                ["returnUrl"] = "https://example.com/away"
            };

            var response = await client.PostAsync("/Trademarks/Add", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                var link = await testDbContext.Set<UserTrademark>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_Add_Unauthenticated_Returns401_AndNoLinkCreated()
        {
            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

            int entityId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

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

            var form = new Dictionary<string, string?> { ["trademarkId"] = entityId.ToString() };

            var response = await client.PostAsync("/Trademarks/Add", new FormUrlEncodedContent(form));

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
        public async Task Post_AddTrademark_SoftDeletedLink_RestoresLink_AndRedirects()
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

                testDbContext.Set<UserTrademark>().Add(new UserTrademark
                {
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = entityId,
                    IsDeleted = true
                });
                await testDbContext.SaveChangesAsync();
            }

            var form = new Dictionary<string, string?>
            {
                ["trademarkId"] = entityId.ToString()
            };

            var response = await client.PostAsync("/Trademarks/Add", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            var uriLocation = response.Headers.Location!;
            var resolvedUri = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);

            resolvedUri.AbsolutePath.Should().Be("/Trademarks/MyCollection");

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var links = await testDbContext.UserTrademarks.AsNoTracking()
                    .Where(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == entityId)
                    .ToListAsync();

                links.Count.Should().Be(1);
                links.Single().IsDeleted.Should().BeFalse();
            }
        }
    }
}
