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

            var uriLocation = response.Headers.Location!;
            var effective = uriLocation.IsAbsoluteUri ? uriLocation : new Uri(client.BaseAddress!, uriLocation);
            effective.AbsolutePath.Should().Be("/Trademarks/MyCollection");

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

    }
}
