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


namespace IPNoticeHub.Tests.IntegrationTests.WatchlistIntegrationTests
{
    [TestFixture]
    public class WatchlistIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;
        private const string TestUserId = "user-1";

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
        public async Task Get_Watchlist_Unauthenticated_Returns401()
        {
            // Skipping CreateClientAs method to simulate an unauthenticated request.
            // TestAuthHandler will return NoResult, treating the user as anonymous.

            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

            var responseMessage = await client.GetAsync("/Watchlist");

            responseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Get_Watchlist_Authenticated_Empty_Returns200()
        {
            var client = appFactory.CreateClientAs(TestUserId);

            var responseMessage = await client.GetAsync("/Watchlist");

            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Post_Add_InsertsRow_AndRedirectsToIndex()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                testDbContext.Users.Add(new ApplicationUser
                {
                    Id = TestUserId,
                    UserName = "testUser",
                    NormalizedUserName = "TESTUSER",
                    Email = "testUser@testemail.com",
                    NormalizedEmail = "TESTUSER@TESTEMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString()
                });

                var entity = new TrademarkEntity
                {
                    Wordmark = "ALPHA",
                    Owner = "Owner A",
                    RegistrationNumber = "11111",
                    GoodsAndServices = "Goods",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };
                testDbContext.TrademarkRegistrations.Add(entity);

                await testDbContext.SaveChangesAsync();
                trademarkId = entity.Id;
            }

            var client = appFactory.CreateClientAs(TestUserId);

            var urlForm = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string,string>("trademarkId", trademarkId.ToString())
    });

            var responseMessage = await client.PostAsync("/Watchlist/Add", urlForm);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.Redirect);
            responseMessage.Headers.Location!.ToString().Should().Be("/Watchlist");

            using var serviceScope = appFactory.Services.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
            var link = dbContext.UserTrademarks.SingleOrDefault(ut =>
                ut.ApplicationUserId == TestUserId && ut.TrademarkRegistrationId == trademarkId);

            link.Should().NotBeNull();
            link!.AddedToWatchlist.Should().BeTrue();
            link.WatchlistNotificationsEnabled.Should().BeFalse();
            link.WatchlistInitialStatusText.Should().Be("Live/Registered");
        }

        [Test]
        public async Task Post_Add_Duplicate_RedirectsBackToReturnUrl_AndDoesNotDuplicate()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                if (!testDbContext.Users.Any(u => u.Id == TestUserId))
                {
                    testDbContext.Users.Add(new ApplicationUser
                    {
                        Id = TestUserId,
                        UserName = "tester",
                        NormalizedUserName = "TESTER",
                        Email = "tester@example.com",
                        NormalizedEmail = "TESTER@EXAMPLE.COM",
                        SecurityStamp = Guid.NewGuid().ToString()
                    });
                }

                var tm = new TrademarkEntity
                {
                    Wordmark = "BETA",
                    Owner = "Owner B",
                    RegistrationNumber = "22222",
                    GoodsAndServices = "Stuff",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(tm);
                await testDbContext.SaveChangesAsync();
                trademarkId = tm.Id;

                testDbContext.UserTrademarks.Add(new UserTrademark
                {
                    ApplicationUserId = TestUserId,
                    TrademarkRegistrationId = trademarkId,
                    AddedToWatchlist = true
                });

                await testDbContext.SaveChangesAsync();
            }

            // Post duplicate add with a returnUrl
            var client = appFactory.CreateClientAs(TestUserId);
            var returnUrl = "/Trademarks/Details/00000000-0000-0000-0000-000000000001";

            var form = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string,string>("trademarkId", trademarkId.ToString()),
        new KeyValuePair<string,string>("returnUrl",   returnUrl)
    });

            var responseMessage = await client.PostAsync("/Watchlist/Add", form);

            // Assert: redirected back to returnUrl, not to /Watchlist
            responseMessage.StatusCode.Should().Be(HttpStatusCode.Redirect);
            responseMessage.Headers.Location!.ToString().Should().Be(returnUrl);

            // Still only one link (no duplication)
            using var verify = appFactory.Services.CreateScope();
            var dbContext = verify.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
            var links = dbContext.UserTrademarks
                           .Where(ut => ut.ApplicationUserId == TestUserId && ut.TrademarkRegistrationId == trademarkId)
                           .ToList();
            links.Count.Should().Be(1);
        }

        [Test]
        public async Task Post_Remove_SoftDeletes_AndRedirectsToIndex()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                if (!db.Users.Any(u => u.Id == TestUserId))
                {
                    db.Users.Add(new ApplicationUser
                    {
                        Id = TestUserId,
                        UserName = "tester",
                        NormalizedUserName = "TESTER",
                        Email = "tester@example.com",
                        NormalizedEmail = "TESTER@EXAMPLE.COM",
                        SecurityStamp = Guid.NewGuid().ToString()
                    });
                }

                var entity = new TrademarkEntity
                {
                    Wordmark = "GAMMA",
                    Owner = "Owner C",
                    RegistrationNumber = "33333",
                    GoodsAndServices = "Things",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };
                db.TrademarkRegistrations.Add(entity);
                await db.SaveChangesAsync();
                trademarkId = entity.Id;

                db.UserTrademarks.Add(new UserTrademark
                {
                    ApplicationUserId = TestUserId,
                    TrademarkRegistrationId = trademarkId,
                    AddedToWatchlist = true,
                    WatchlistNotificationsEnabled = true
                });

                await db.SaveChangesAsync();
            }

            var client = appFactory.CreateClientAs(TestUserId);
            var form = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string,string>("trademarkId", trademarkId.ToString())
    });

            var response = await client.PostAsync("/Watchlist/Remove", form);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Be("/Watchlist");

            using (var verifyScope = appFactory.Services.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                var link = await dbContext.UserTrademarks
                    .SingleOrDefaultAsync(ut => ut.ApplicationUserId == TestUserId &&
                                                ut.TrademarkRegistrationId == trademarkId);

                link.Should().NotBeNull("the relationship row should still exist (soft delete)");
                link!.AddedToWatchlist.Should().BeFalse();
                link.WatchlistNotificationsEnabled.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_ToggleNotifications_OnAndOff_Persists()
        {
            // arrange
            var userId = "user-toggle-1";
            var client = appFactory.CreateClientAs(userId);

            int trademarkId;
            using (var scope = appFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                db.Users.Add(new ApplicationUser { Id = userId, UserName = "tester", Email = "tester@example.com" });
                var tm = new TrademarkEntity
                {
                    Wordmark = "NOTIFY-MARK",
                    SourceId = "US-987654321",
                    RegistrationNumber = "RN-9999",
                    GoodsAndServices = "Widgets",
                    Owner = "Notify Corp",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending examination",
                    Source = DataProvider.USPTO
                };
                db.TrademarkRegistrations.Add(tm);
                await db.SaveChangesAsync();
                trademarkId = tm.Id;
            }

            // local helper to read the link
            async Task<UserTrademark?> ReadLinkAsync()
            {
                using var scope = appFactory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                return await db.UserTrademarks.AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == trademarkId);
            }

            // add to watchlist
            var addForm = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("trademarkId", trademarkId.ToString())
    });
            (await client.PostAsync("/Watchlist/Add", addForm)).StatusCode.Should().Be(HttpStatusCode.Redirect);

            (await ReadLinkAsync()).Should().NotBeNull();
            (await ReadLinkAsync())!.WatchlistNotificationsEnabled.Should().BeFalse();

            // toggle ON
            var onForm = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("trademarkId", trademarkId.ToString()),
        new KeyValuePair<string, string>("notificationsEnabled", "true")
    });
            (await client.PostAsync("/Watchlist/ToggleNotifications", onForm))
                .StatusCode.Should().Be(HttpStatusCode.Redirect);
            (await ReadLinkAsync())!.WatchlistNotificationsEnabled.Should().BeTrue();

            // toggle OFF
            var offForm = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("trademarkId", trademarkId.ToString()),
        new KeyValuePair<string, string>("notificationsEnabled", "false")
    });
            (await client.PostAsync("/Watchlist/ToggleNotifications", offForm))
                .StatusCode.Should().Be(HttpStatusCode.Redirect);
            (await ReadLinkAsync())!.WatchlistNotificationsEnabled.Should().BeFalse();
        }



    }
}
