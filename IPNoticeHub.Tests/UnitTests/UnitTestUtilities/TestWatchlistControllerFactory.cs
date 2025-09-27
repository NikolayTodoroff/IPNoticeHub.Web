using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Services.Application.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.UnitTestUtilities
{
    public static class TestWatchlistControllerFactory
    {
        public static ClaimsPrincipal CreateNewUser(string userId = "user-1")
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "tester")
            },  authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        public static (WatchlistController ctrl, Mock<ITrademarkWatchlistService> svc, DefaultHttpContext http)
            CreateWatchlistController(bool userExists = true)
        {
            var watchlistService = new Mock<ITrademarkWatchlistService>(MockBehavior.Strict);
            var watchlistController = new WatchlistController(watchlistService.Object);

            var httpContext = new DefaultHttpContext();

            if (userExists)
            {
                httpContext.User = CreateNewUser();
            }

            var tempDataProvider = new Mock<ITempDataProvider>();
            watchlistController.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            watchlistController.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return (watchlistController, watchlistService, httpContext);
        }
    }
}
