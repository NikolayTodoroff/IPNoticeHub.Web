using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Implementations;
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
    /// Provides utility methods to create and configure instances of TrademarksController for unit testing.
    /// </summary>
    public static class TestTrademarkControllerFactory
    {
        /// <summary>
        /// Creates a fully configured instance of TrademarksController with TempData and UrlHelper.
        /// This version is typically used in tests that require TempData and UrlHelper,
        /// such as TmControllerAddRemoveTests.
        /// </summary>
        public static TrademarksController CreateTrademarksController(
            ITrademarkCollectionService collectionService,
            out ITempDataDictionary tempData,
            ITrademarkSearchService? searchService = null,
            ITrademarkWatchlistService? watchlistService = null,
            string? userId = null)
        {
            return CreateTrademarksControllerCore(
                collectionService,
                searchService,
                watchlistService,
                userId,
                out tempData,
                includeTempData: true,
                includeUrlHelper: true);
        }

        /// <summary>
        /// Creates a simplified instance of TrademarksController without TempData and UrlHelper.
        /// This version is typically used in tests that do not require TempData or UrlHelper, 
        /// such as TmControllerMyCollectionTests.
        /// </summary>
        public static TrademarksController CreateTrademarksController(
            ITrademarkCollectionService collectionService,
            ITrademarkSearchService? searchService = null,
            ITrademarkWatchlistService? watchlistService = null,
            string? userId = null)
        {
            return CreateTrademarksControllerCore(
                collectionService,
                searchService,
                watchlistService,
                userId,
                out _,
                includeTempData: true,
                includeUrlHelper: false);
        }

        /// <summary>
        /// Core method for creating and configuring an instance of TrademarksController.
        /// Allows customization of TempData and UrlHelper inclusion.
        /// </summary>
        private static TrademarksController CreateTrademarksControllerCore(
            ITrademarkCollectionService collectionService,
            ITrademarkSearchService? searchService,
            ITrademarkWatchlistService? watchlistService,
            string? userId,
            out ITempDataDictionary tempData,
            bool includeTempData,
            bool includeUrlHelper)
        {
            var httpContext = new DefaultHttpContext();
            var pdfService = new Mock<IPdfService>();
            var letterTemplate = new Mock<ILetterTemplateProvider>();

            // Simulate an authenticated user if a user ID is provided.
            if (!string.IsNullOrEmpty(userId))
            {
                httpContext.User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"));
            }

            var controller = new TrademarksController(
                searchService ?? Mock.Of<ITrademarkSearchService>(),
                collectionService,
                watchlistService ?? Mock.Of<ITrademarkWatchlistService>(),
                pdfService.Object,
                letterTemplate.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Configure TempData if requested.
            if (includeTempData)
            {
                tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
                controller.TempData = tempData;
            }
            else
            {
                tempData = null!;
            }

            // Configure UrlHelper if requested.
            if (includeUrlHelper)
            {
                var urlHelperMock = new Mock<IUrlHelper>();
                urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                    .Returns<string>(url => !string.IsNullOrEmpty(url) && url.StartsWith("/"));

                controller.Url = urlHelperMock.Object;
            }

            return controller;
        }  
    }
}
