using IPNoticeHub.Application.Watchlist.Abstractions;
using IPNoticeHub.Application.Watchlist.Implementations;
using IPNoticeHub.Application.DocumentLibrary.Abstractions;
using IPNoticeHub.Application.PdfGeneration.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
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
            string? userId = null,
            IDocumentLibraryService? documentLibraryService = null)
        {
            return CreateTrademarksControllerCore(
                collectionService,
                searchService,
                watchlistService,
                userId,
                out tempData,
                includeTempData: true,
                includeUrlHelper: true,
                documentLibraryService: documentLibraryService);
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
            string? userId = null,
            IDocumentLibraryService? documentLibraryService = null)
        {
            return CreateTrademarksControllerCore(
                collectionService,
                searchService,
                watchlistService,
                userId,
                out _,
                includeTempData: true,
                includeUrlHelper: false,
                documentLibraryService: documentLibraryService);
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
            bool includeUrlHelper,
            IDocumentLibraryService? documentLibraryService = null)
        {
            var httpContext = new DefaultHttpContext();
            var pdfService = new Mock<IPdfService>();
            var letterTemplate = new Mock<ILetterTemplateProvider>();
            var docLibraryService = documentLibraryService ?? Mock.Of<IDocumentLibraryService>();

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
                letterTemplate.Object,
                docLibraryService)
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
