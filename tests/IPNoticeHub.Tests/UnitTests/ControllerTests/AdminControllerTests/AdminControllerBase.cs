using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Security.Claims;
using IPNoticeHub.Shared.Enums;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.AdminControllerTests
{
    public class AdminControllerBase
    {
        protected DbContextOptions<IPNoticeHubDbContext> dbContextOptions = null!;
        protected IPNoticeHubDbContext testDbContext = null!;
        protected AdminController controller = null!;

        [SetUp]
        public void SetUp()
        {
            dbContextOptions = new DbContextOptionsBuilder<IPNoticeHubDbContext>().
                UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).
                EnableSensitiveDataLogging().
                Options;

            testDbContext = new IPNoticeHubDbContext(dbContextOptions);

            var users = 
                Enumerable.Range(1, 7).Select(
                i => new ApplicationUser
            {
                Id = $"user0{i}",                      
                UserName = $"user{i}@test.io",
                Email = $"user{i}@test.io"
            }).
            ToList();

            testDbContext.Users.AddRange(users);

            testDbContext.TrademarkRegistrations.AddRange( 
                new TrademarkEntity 
                { 
                    Id = 1, 
                    Owner = "user1",
                    Wordmark = "Test Wordmark 1",
                    SourceId = "11s1",
                    RegistrationNumber = "123456",
                    GoodsAndServices = "Software",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Approved",
                    Source = DataProvider.USPTO
                },
                new TrademarkEntity 
                {
                    Id = 2,
                    Owner = "user2",
                    Wordmark = "Test Wordmark 2",
                    SourceId = "22s2",
                    RegistrationNumber = "654321",
                    GoodsAndServices = "Software",
                    StatusCategory = TrademarkStatusCategory.Pending,
                    StatusDetail = "Pending Final Approval",
                    Source = DataProvider.USPTO
                },
                new TrademarkEntity 
                {
                    Id = 3,
                    Owner = "user3",
                    Wordmark = "Test Wordmark 3",
                    SourceId = "33s3",
                    RegistrationNumber = "44332211",
                    GoodsAndServices = "Software",
                    StatusCategory = TrademarkStatusCategory.Cancelled,
                    StatusDetail = "Renewal Deadline Passed",
                    Source = DataProvider.USPTO
                },
                new TrademarkEntity
                {
                    Id = 4,
                    Owner = "user4",
                    Wordmark = "Test Wordmark 4",
                    SourceId = "44s4",
                    RegistrationNumber = "667744",
                    GoodsAndServices = "Software",
                    StatusCategory = TrademarkStatusCategory.Registered,
                    StatusDetail = "Successfully Registered",
                    Source = DataProvider.USPTO
                });

            testDbContext.CopyrightRegistrations.AddRange(
                new CopyrightEntity 
                { 
                    Id = 1,
                    RegistrationNumber = "123465",
                    TypeOfWork = "Computer Software",
                    Title = "Copyright Entity 1",
                    Owner = "TestOwner1" 
                },
                new CopyrightEntity 
                {
                    Id = 2,
                    RegistrationNumber = "332211",
                    TypeOfWork = "Computer Software",
                    Title = "Copyright Entity 2",
                    Owner = "TestOwner2"
                });

            testDbContext.Watchlists.AddRange(
                new Watchlist 
                { 
                    Id = 100, 
                    UserId = users[0].Id,
                    TrademarkId = 1
                },
                new Watchlist 
                { 
                    Id = 101, 
                    UserId = users[1].Id,
                    TrademarkId = 2
                },
                new Watchlist 
                { 
                    Id = 102, 
                    UserId = users[2].Id,
                    TrademarkId = 3
                },
                new Watchlist 
                { 
                    Id = 103,
                    UserId = users[3].Id,
                    TrademarkId = 4
                });

            testDbContext.SaveChanges();

            controller = new AdminController(testDbContext);

            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "admin-user")
                }, "test"))
            };

            controller.ControllerContext = new ControllerContext { HttpContext = http };

            controller.TempData = new TempDataDictionary(
                http, 
                new FakeTempDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            testDbContext?.Dispose();
        }
    }

    public class FakeTempDataProvider : ITempDataProvider
    {
        private Dictionary<string, object> tempData = new();
        
        public IDictionary<string, object> LoadTempData(HttpContext context) => tempData;
        
        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            tempData = new Dictionary<string, object>(values);
        }
    }
}
