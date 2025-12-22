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

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.AdminControllerTests
{
    public class AdminControllerBase
    {
        private DbContextOptions<IPNoticeHubDbContext> dbContextOptions = null!;
        private IPNoticeHubDbContext testDbContext = null!;
        private AdminController controller = null!;

        [SetUp]
        public void SetUp()
        {
            dbContextOptions = new DbContextOptionsBuilder<IPNoticeHubDbContext>().
                UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).
                EnableSensitiveDataLogging().
                Options;

            testDbContext = new IPNoticeHubDbContext(dbContextOptions);

            var users = Enumerable.Range(1, 7).Select(
                i => new ApplicationUser
            {
                Id = $"user0{i}",                      
                UserName = $"user{i}@test.io",
                Email = $"user{i}@test.io"
            }).
            ToList();

            testDbContext.Users.AddRange(users);

            testDbContext.TrademarkRegistrations.AddRange(
                new TrademarkEntity { Id = 1, Owner = "o1" },
                new TrademarkEntity { Id = 2, Owner = "o2" },
                new TrademarkEntity { Id = 3, Owner = "o3" });

            testDbContext.CopyrightRegistrations.AddRange(
                new CopyrightEntity { Id = 10, Owner = "c1" },
                new CopyrightEntity { Id = 11, Owner = "c2" });

            testDbContext.Watchlists.AddRange(
                new Watchlist { Id = 100, UserId = users[0].Id },
                new Watchlist { Id = 101, UserId = users[1].Id },
                new Watchlist { Id = 102, UserId = users[2].Id },
                new Watchlist { Id = 103, UserId = users[3].Id });

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
        private Dictionary<string, object> _data = new();
        
        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
        
        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            _data = new Dictionary<string, object>(values);
        }
    }
}
