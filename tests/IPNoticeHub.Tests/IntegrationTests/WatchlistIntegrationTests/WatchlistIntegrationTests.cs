using FluentAssertions;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;

namespace IPNoticeHub.Tests.IntegrationTests.WatchlistIntegrationTests
{
    [NonParallelizable]
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
            var client = appFactory.CreateClient(
                new() { AllowAutoRedirect = false });

            var responseMessage = 
                await client.GetAsync("/Watchlist");

            responseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Get_Watchlist_Authenticated_Empty_Returns200()
        {
            var client = appFactory.CreateClientAs(TestUserId);

            var responseMessage = 
                await client.GetAsync("/Watchlist");

            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Post_Add_InsertsRow_AndRedirectsToIndex()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    scope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

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
                    SourceId = "TestSourceId",
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

            var urlForm = new FormUrlEncodedContent(
                new[]{ new KeyValuePair<string,string>("trademarkId",trademarkId.ToString())});

            var responseMessage = 
                await client.PostAsync("/Watchlist/Add", urlForm);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.Redirect);
            responseMessage.Headers.Location!.ToString().Should().Be("/Watchlist");

            using var serviceScope = appFactory.Services.CreateScope();

            var dbContext = 
                serviceScope.ServiceProvider.
                GetRequiredService<IPNoticeHubDbContext>();

            var watchlist = dbContext.Watchlists.SingleOrDefault(
                    ut => ut.UserId == TestUserId && 
                    ut.TrademarkId == trademarkId);

            watchlist.Should().NotBeNull();
            watchlist.NotificationsEnabled.Should().BeFalse();
            watchlist.InitialStatusText.Should().Be("Live/Registered");
        }

        [Test]
        public async Task Post_Add_WhenWatchlistRowExists_UpdatesMissingFields_AndUndeletes()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

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
                    SourceId = "TestSourceId",
                    RegistrationNumber = "1111",
                    GoodsAndServices = "Goods",
                    StatusCategory = TrademarkStatusCategory.Registered,

                    StatusDetail = "Live/Registered",
                    StatusCodeRaw = 630,
                    StatusDateUtc = new DateTime(
                        2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();

                trademarkId = entity.Id;

                testDbContext.Watchlists.Add(new Watchlist
                {
                    UserId = TestUserId,
                    TrademarkId = trademarkId,

                    IsDeleted = true,
                    NotificationsEnabled = false,

                    AddedOnUtc = default,
                    InitialStatusCodeRaw = null,
                    InitialStatusText = null,
                    InitialStatusDateUtc = null
                });

                await testDbContext.SaveChangesAsync();
            }

            var client = appFactory.CreateClientAs(TestUserId);

            var urlForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("trademarkId", trademarkId.ToString())
            });

            var response = 
                await client.PostAsync("/Watchlist/Add", urlForm);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Be("/Watchlist");

            using var verifyScope = appFactory.Services.CreateScope();

            var verifyDbContext = 
                verifyScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

            var watchlist = verifyDbContext.Watchlists.SingleOrDefault(w =>
                w.UserId == TestUserId && w.TrademarkId == trademarkId);

            watchlist.Should().NotBeNull();

            watchlist!.IsDeleted.Should().BeFalse(
                "existing soft-deleted entries should be undeleted on Add");
            
            watchlist.NotificationsEnabled.Should().BeFalse();

            watchlist.AddedOnUtc.Should().NotBe(default);

            watchlist.InitialStatusCodeRaw.Should().Be(630);
            watchlist.InitialStatusText.Should().Be("Live/Registered");
            watchlist.InitialStatusDateUtc.Should().Be(
                new DateTime(2025, 1, 1, 
                0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public async Task CountByUserAsync_WithThreeWatchlistEntries_ReturnsThree()
        {
            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext =
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                if (!await testDbContext.Users.AnyAsync(u => u.Id == TestUserId))
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

                var entity1 = new TrademarkEntity
                {
                    Wordmark = "firstTm",
                    Owner = "Owner A",
                    SourceId = "TestSourceId1",
                    RegistrationNumber = "123456",
                    GoodsAndServices = "Goods",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };

                var entity2 = new TrademarkEntity
                {
                    Wordmark = "secondTm",
                    Owner = "Owner B",
                    SourceId = "TestSourceId2",
                    RegistrationNumber = "654321",
                    GoodsAndServices = "Services",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };

                var entity3 = new TrademarkEntity
                {
                    Wordmark = "thirdTm",
                    Owner = "Owner C",
                    SourceId = "TestSourceId3",
                    RegistrationNumber = "654124",
                    GoodsAndServices = "Services",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.AddRange(entity1, entity2, entity3);
                await testDbContext.SaveChangesAsync();

                testDbContext.Watchlists.AddRange(
                    new Watchlist
                    {
                        UserId = TestUserId,
                        TrademarkId = entity1.Id,
                        NotificationsEnabled = true
                    },

                    new Watchlist
                    {
                        UserId = TestUserId,
                        TrademarkId = entity2.Id,
                        NotificationsEnabled = true
                    },

                    new Watchlist
                    {
                        UserId = TestUserId,
                        TrademarkId = entity3.Id,
                        NotificationsEnabled = true
                    });

                await testDbContext.SaveChangesAsync();
            }

            using (var verifyScope = appFactory.Services.CreateScope())
            {
                var repository = 
                    verifyScope.ServiceProvider.GetRequiredService<IWatchlistRepository>();

                var count = 
                    await repository.CountByUserAsync(TestUserId,CancellationToken.None);

                count.Should().Be(3);
            }
        }

        [Test]
        public async Task Post_Add_Duplicate_RedirectsBackToReturnUrl_AndDoesNotDuplicate()
        {
            int trademarkId;

            using (var scope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    scope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

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

                var entity = new TrademarkEntity
                {
                    Wordmark = "BETA",
                    Owner = "Owner B",
                    GoodsAndServices = "testGoodsAndSerices",
                    SourceId = "testSourceId",
                    StatusDetail = "Live/Registered",
                    RegistrationNumber = "22222",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                trademarkId = entity.Id;

                testDbContext.Watchlists.Add(
                    new Watchlist
                {
                    UserId = TestUserId,
                    TrademarkId = trademarkId
                });

                await testDbContext.SaveChangesAsync();
            }

            var client = appFactory.CreateClientAs(TestUserId);

            var returnUrl = 
                "/Trademarks/Details/00000000-0000-0000-0000-000000000001";

            var form = new FormUrlEncodedContent(new[]
            {
                 new KeyValuePair<string,string>("trademarkId",trademarkId.ToString()),
                 new KeyValuePair<string,string>("returnUrl", returnUrl)
            });

            var responseMessage = 
                await client.PostAsync("/Watchlist/Add",form);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.Redirect);
            responseMessage.Headers.Location!.ToString().Should().Be(returnUrl);

            using var verify = appFactory.Services.CreateScope();

            var dbContext = 
                verify.ServiceProvider.
                GetRequiredService<IPNoticeHubDbContext>();

            var links = 
                dbContext.Watchlists.Where(
                    ut => ut.UserId == TestUserId && 
                    ut.TrademarkId == trademarkId).
                    ToList();

            links.Count.Should().Be(1);
        }

        [Test]
        public async Task Post_Remove_SoftDeletes_AndRedirectsToIndex()
        {
            int trademarkId;

            using (var serviceScope = appFactory.Services.CreateScope())
            {
                var testDbContext = 
                    serviceScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

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

                var entity = new TrademarkEntity
                {
                    Wordmark = "GAMMA",
                    Owner = "Owner C",
                    SourceId = "TestSourceId",
                    RegistrationNumber = "33333",
                    GoodsAndServices = "Things",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Live/Registered",
                    Source = DataProvider.USPTO
                };

                testDbContext.TrademarkRegistrations.Add(entity);
                await testDbContext.SaveChangesAsync();
                trademarkId = entity.Id;

                testDbContext.Watchlists.Add(
                    new Watchlist
                {
                    UserId = TestUserId,
                    TrademarkId = trademarkId,
                    NotificationsEnabled = true
                });

                await testDbContext.SaveChangesAsync();
            }

            var client = appFactory.CreateClientAs(TestUserId);

            var form = new FormUrlEncodedContent(
            new[]{ new KeyValuePair<string,string>("trademarkId",trademarkId.ToString())});

            var response = 
                await client.PostAsync("/Watchlist/Remove", form);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);

            response.Headers.Location!.ToString().
                Should().MatchRegex(@"^/Watchlist(/Index)?/?(\?.*)?$");

            using (var verifyScope = appFactory.Services.CreateScope())
            {
                var dbContext = 
                    verifyScope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                var link = await dbContext.Watchlists.
                    IgnoreQueryFilters().
                    SingleOrDefaultAsync(
                    ut => ut.UserId == TestUserId && 
                    ut.TrademarkId == trademarkId);

                link.Should().NotBeNull();
                link!.IsDeleted.Should().BeTrue();
                link.NotificationsEnabled.Should().BeFalse();
            }
        }

        [Test]
        public async Task Post_ToggleNotifications_OnAndOff_Persists()
        {
            var userId = "user-toggle-1";
            var client = appFactory.CreateClientAs(userId);

            int trademarkId;
            using (var scope = appFactory.Services.CreateScope())
            {
                var dbContext = 
                    scope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                dbContext.Users.Add(
                    new ApplicationUser { 
                        Id = userId, 
                        UserName = "tester", 
                        Email = "tester@example.com" });

                var entity = new TrademarkEntity
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

                dbContext.TrademarkRegistrations.Add(entity);
                await dbContext.SaveChangesAsync();
                trademarkId = entity.Id;
            }

            async Task<Watchlist?> ReadLinkAsync()
            {
                using var scope = appFactory.Services.CreateScope();

                var dbContext = 
                    scope.ServiceProvider.
                    GetRequiredService<IPNoticeHubDbContext>();

                return await dbContext.Watchlists.
                    AsNoTracking().
                    FirstOrDefaultAsync(
                    ut => ut.UserId == userId && 
                    ut.TrademarkId == trademarkId);
            }

            var addForm = new FormUrlEncodedContent(
                new[]{new KeyValuePair<string,string>("trademarkId", trademarkId.ToString())
            });

            (await client.PostAsync("/Watchlist/Add", addForm)).StatusCode.
                Should().Be(HttpStatusCode.Redirect);

            (await ReadLinkAsync()).Should().NotBeNull();
            (await ReadLinkAsync())!.NotificationsEnabled.Should().BeFalse();

            var onForm = new FormUrlEncodedContent(
                new[]{new KeyValuePair<string,string>("trademarkId", trademarkId.ToString()),
                new KeyValuePair<string,string>("enabled", "true")
            });

            (await client.PostAsync("/Watchlist/ToggleNotifications", onForm)).StatusCode.
                Should().Be(HttpStatusCode.Redirect);

            (await ReadLinkAsync())!.NotificationsEnabled.Should().BeTrue();

            var offForm = new FormUrlEncodedContent(
                new[]{new KeyValuePair<string,string>("trademarkId",trademarkId.ToString()), 
                new KeyValuePair<string,string>("enabled", "false")
            });

            (await client.PostAsync(
                "/Watchlist/ToggleNotifications", offForm)).StatusCode.
                Should().Be(HttpStatusCode.Redirect);

            (await ReadLinkAsync())!.NotificationsEnabled.Should().BeFalse();
        }
    }
}