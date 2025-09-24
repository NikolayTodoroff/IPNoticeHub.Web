using System.Security.Claims;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{
    /// <summary>
    /// Utilities to create configured HomeController instances for tests.
    /// </summary>
    public static class TestHomeControllerFactory
    {
        /// <summary>
        /// Creates a HomeController and wires TempData (and optionally UrlHelper).
        /// Use this when a test needs TempData and/or Url behaviour.
        /// </summary>
        public static HomeController CreateHomeController(
            ITrademarkSearchQueryService searchQueryService,
            out ITempDataDictionary tempData,
            string? userId = null,
            bool includeUrlHelper = false)
        {
            return CreateHomeControllerCore(
                searchQueryService,
                userId,
                out tempData,
                includeTempData: true,
                includeUrlHelper: includeUrlHelper);
        }

        public static HomeController CreateHomeController(
            ITrademarkSearchQueryService searchQueryService, string? userId = null)
        {
            return CreateHomeControllerCore(
                searchQueryService,
                userId,
                out _,
                includeTempData: false,
                includeUrlHelper: false);
        }

        private static HomeController CreateHomeControllerCore(
            ITrademarkSearchQueryService searchQueryService,
            string? userId,
            out ITempDataDictionary tempData,
            bool includeTempData,
            bool includeUrlHelper)
        {
            var httpContext = new DefaultHttpContext();

            // Simulate an authenticated user if requested.
            if (!string.IsNullOrEmpty(userId))
            {
                httpContext.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"));
            }

            var controller = new HomeController(searchQueryService)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            if (includeTempData)
            {
                tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
                controller.TempData = tempData;
            }

            else
            {
                tempData = null!;
            }

            if (includeUrlHelper)
            {
                var urlHelperMock = new Mock<IUrlHelper>();
                urlHelperMock
                    .Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                    .Returns<string>(url => !string.IsNullOrWhiteSpace(url) && url.StartsWith("/"));

                controller.Url = urlHelperMock.Object;
            }

            return controller;
        }
    }
}
