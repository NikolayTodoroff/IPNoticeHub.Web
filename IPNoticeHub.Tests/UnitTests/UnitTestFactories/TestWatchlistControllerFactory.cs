using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.UnitTestFactories
{
    public static class TestWatchlistControllerFactory
    {
        public static ClaimsPrincipal CreateNewUser(string userId = "user-1")
            => new(new ClaimsIdentity(new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, 
                    userId),

                new Claim(
                    ClaimTypes.Name, 
                    "tester")
            }, "TestAuth"));


        public static (
            WatchlistController controller, 
            Mock<IWatchlistService> service, 
            DefaultHttpContext http)
            CreateWatchlistController(bool userExists = true)
        {
            var service = 
                new Mock<IWatchlistService>(MockBehavior.Strict);

            var controller = 
                new WatchlistController(service.Object);

            var httpContext = new DefaultHttpContext();

            if (userExists) httpContext.User = CreateNewUser();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData()
            };

            controller.TempData = new TempDataDictionary(
                httpContext, 
                Mock.Of<ITempDataProvider>());

            controller.ConfigureUrlHelper();

            return (controller, service, httpContext);
        }

        public static (
            WatchlistController controller, 
            Mock<IWatchlistService> service, DefaultHttpContext httpContext)
            CreateWatchlistControllerWithReturnUrl(
            string returnUrl, 
            bool isLocal = true, 
            bool userExists = true)
        {
            var (controller, svc, http) = 
                CreateWatchlistController(userExists);

            controller.ConfigureUrlHelper(returnUrl, isLocal);
            return (controller, svc, http);
        }

        public static void ConfigureUrlHelper(
            this Controller controller, 
            string? returnUrl = null, 
            bool isLocal = true)
        {
            var url = new Mock<IUrlHelper>();

            if (returnUrl is not null)
            {
                url.Setup(u => u.IsLocalUrl(returnUrl)).
                    Returns(isLocal);
            }

            else
            {
                url.Setup(u => u.IsLocalUrl(It.IsAny<string?>()))
                   .Returns<string?>(
                    u => !string.IsNullOrEmpty(u) && 
                    u.StartsWith("/"));
            }

            controller.Url = url.Object;
        }
    }
}
