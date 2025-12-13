using System.Security.Claims;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationService.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{
    public static class TestTrademarkControllerFactory
    {
        public static TrademarksController CreateTrademarksController(
            ITrademarkCollectionService collectionService,
            out ITempDataDictionary tempData,
            ITrademarkSearchService? searchService = null,
            IWatchlistService? watchlistService = null,
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

        public static TrademarksController CreateTrademarksController(
            ITrademarkCollectionService collectionService,
            ITrademarkSearchService? searchService = null,
            IWatchlistService? watchlistService = null,
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

        private static TrademarksController CreateTrademarksControllerCore(
            ITrademarkCollectionService collectionService,
            ITrademarkSearchService? searchService,
            IWatchlistService? watchlistService,
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

            if (!string.IsNullOrEmpty(userId))
            {
                httpContext.User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth"));
            }

            var controller = new TrademarksController(
                searchService ?? Mock.Of<ITrademarkSearchService>(),
                collectionService,
                watchlistService ?? Mock.Of<IWatchlistService>(),
                pdfService.Object,
                letterTemplate.Object,
                docLibraryService)
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
                urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                    .Returns<string>(url => !string.IsNullOrEmpty(url) && url.StartsWith("/"));

                controller.Url = urlHelperMock.Object;
            }

            return controller;
        }  
    }
}
