using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    public class TrademarkDetailsIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Get_Details_Existing_Returns200()
        {
            Guid publicId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var entity = new TrademarkEntity
                {
                    Wordmark = "TESTMARK",
                    SourceId = "US-123456789",
                    RegistrationNumber = "RN-0001",
                    GoodsAndServices = "Software",
                    Owner = "Prey Corp",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                publicId = entity.PublicId;
            }

            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

            var response = await client.GetAsync($"/Trademarks/Details/{publicId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Get_Details_WithMissingEntityId_Returns404()
        {
            var missingId = Guid.NewGuid();
            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

            var response = await client.GetAsync($"/Trademarks/Details/{missingId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Get_MyCollection_WithLinkedItems_Returns200()
        {
            var userId = "u1";
            var client = appFactory.CreateClientAs(userId);

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                await TestDbSeeder.SeedUserAsync(testDbContext, userId);

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "FIRST WM",
                    SourceId = "US-ALPHA-001",
                    RegistrationNumber = "RN-A1",
                    GoodsAndServices = "Software; services",
                    Owner = "Mortal Kombat Inc.",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "LAST WM",
                    SourceId = "US-BRAVO-002",
                    RegistrationNumber = "RN-B2",
                    GoodsAndServices = "Games; media",
                    Owner = "Street Fighters LLC",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Registered",
                    FilingDate = null,
                    RegistrationDate = null,
                    MarkImageUrl = null,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
                await testDbContext.SaveChangesAsync();

                testDbContext.Set<UserTrademark>().AddRange(
                    new UserTrademark { ApplicationUserId = userId, TrademarkRegistrationId = entity1.Id, IsDeleted = false },
                    new UserTrademark { ApplicationUserId = userId, TrademarkRegistrationId = entity2.Id, IsDeleted = false }
                );

                await testDbContext.SaveChangesAsync();
            }

            var response = await client.GetAsync("/Trademarks/MyCollection");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
