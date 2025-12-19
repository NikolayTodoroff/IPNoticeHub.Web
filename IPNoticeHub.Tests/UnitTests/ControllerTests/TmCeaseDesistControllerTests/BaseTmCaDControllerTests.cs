using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCeaseDesistControllerTests
{
    public abstract class BaseTmCaDControllerTests
    {
        protected const string TestUserId = "test-user-id";

        protected Mock<ITrademarkSearchService> trademarkSearchService = null!;
        protected Mock<ITrademarkCollectionService> trademarkCollectionService = null!;
        protected Mock<IPdfLetterService> pdfService = null!;
        protected Mock<ILetterTemplateProvider> letterTemplateProvider = null!;
        protected Mock<IDocumentLibraryService> documentLibraryService = null!;
        protected Mock<ITemplateTokenReplacer> templateReplacer = null!;
        protected TrademarkCadController controller = null!;

        [SetUp]
        public void BaseSetUp()
        {
            trademarkSearchService = new Mock<ITrademarkSearchService>();
            trademarkCollectionService = new Mock<ITrademarkCollectionService>();
            pdfService = new Mock<IPdfLetterService>();
            letterTemplateProvider = new Mock<ILetterTemplateProvider>();
            documentLibraryService = new Mock<IDocumentLibraryService>();
            templateReplacer = new Mock<ITemplateTokenReplacer>();

            controller = new TrademarkCadController(
                trademarkSearchService.Object,
                trademarkCollectionService.Object,
                pdfService.Object,
                letterTemplateProvider.Object,
                documentLibraryService.Object,
                templateReplacer.Object
            );

            SetupControllerContext();
        }

        protected void SetupControllerContext()
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]{new Claim(ClaimTypes.NameIdentifier, TestUserId)}, 
                        "TestAuth"))
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = 
                new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }
    }
}
