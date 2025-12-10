using IPNoticeHub.Application.Watchlist.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.UnitTestUtilities
{
    public static class TestWatchlistControllerFactory
    {
        public static ClaimsPrincipal CreateNewUser(string userId = "user-1")
            => new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "tester")
            }, "TestAuth"));

        /// <summary>
        /// Creates a WatchlistController with:
        /// - Mock<ITrademarkWatchlistService> (Strict),
        /// - HttpContext (optionally with user),
        /// - TempData, RouteData,
        /// - UrlHelper that treats any "/..." as local.
        /// </summary>
        public static (WatchlistController controller, Mock<ITrademarkWatchlistService> svc, DefaultHttpContext http)
            CreateWatchlistController(bool userExists = true)
        {
            var svc = new Mock<ITrademarkWatchlistService>(MockBehavior.Strict);
            var ctrl = new WatchlistController(svc.Object);

            var http = new DefaultHttpContext();
            if (userExists)
            {
                http.User = CreateNewUser();
            }

            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = http,
                RouteData = new RouteData()
            };

            ctrl.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());

            // Safe default: treat "/..." as local so Url.IsLocalUrl(...) never NREs.
            ctrl.ConfigureUrlHelper();

            return (ctrl, svc, http);
        }

        /// <summary>
        /// Convenience creator when the test needs a specific returnUrl to be local/non-local.
        /// </summary>
        public static (WatchlistController controller, Mock<ITrademarkWatchlistService> svc, DefaultHttpContext http)
            CreateWatchlistControllerWithReturnUrl(string returnUrl, bool isLocal = true, bool userExists = true)
        {
            var (controller, svc, http) = CreateWatchlistController(userExists);
            controller.ConfigureUrlHelper(returnUrl, isLocal);
            return (controller, svc, http);
        }

        /// <summary>
        /// Ensures controller.Url.IsLocalUrl(...) is safe in unit tests.
        /// If returnUrl is provided, IsLocalUrl(returnUrl) returns 'isLocal';
        /// otherwise any string starting with '/' is treated as local.
        /// </summary>
        public static void ConfigureUrlHelper(this Controller controller, string? returnUrl = null, bool isLocal = true)
        {
            var url = new Mock<IUrlHelper>();

            if (returnUrl is not null)
            {
                url.Setup(u => u.IsLocalUrl(returnUrl)).Returns(isLocal);
            }
            else
            {
                url.Setup(u => u.IsLocalUrl(It.IsAny<string?>()))
                   .Returns<string?>(u => !string.IsNullOrEmpty(u) && u.StartsWith("/"));
            }

            controller.Url = url.Object;
        }
    }
}
