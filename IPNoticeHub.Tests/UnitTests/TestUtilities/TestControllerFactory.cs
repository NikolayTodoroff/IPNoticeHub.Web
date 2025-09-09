using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{
    /// <summary>
    /// Provides utility methods for creating and configuring instances of controllers for unit testing.
    /// </summary>
    public static class TestControllerFactory
    {
        public static TrademarksController CreateTrademarksController(ITrademarkCollectionService tmCollectionService,
            out ITempDataDictionary tempData, string? userId = null, ITrademarkSearchService? tmSearchService = null)
        {
            var httpContext = new DefaultHttpContext();

            // Simulate an authenticated user if a userId is provided
            if (!string.IsNullOrEmpty(userId))
            {
                var identity = new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                    authenticationType: "TestAuth");

                httpContext.User = new ClaimsPrincipal(identity);
            }

            // Create the controller with the provided or mocked services
            var controller = new TrademarksController(
                tmSearchService ?? Mock.Of<ITrademarkSearchService>(), tmCollectionService);

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Configure TempData for the controller
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Configure a mocked UrlHelper for the controller
            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                .Returns<string>(url => !string.IsNullOrEmpty(url) && url.StartsWith("/"));
            controller.Url = urlHelperMock.Object;

            return controller;
        }
    }
}
