using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Constants;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public abstract class BaseCopyrightCadControllerTests
    {
        protected const string UserId = "user-123";
        protected static readonly Guid PublicId = Guid.NewGuid();

        protected const string KeySpace = 
            InputDraftConstants.UserInputDraftOptions.CopyrightCadKeySpace;

        protected Mock<ICopyrightService> copyrightService = null!;
        protected Mock<IPdfLetterService> pdfService = null!;
        protected Mock<ILetterTemplateProvider> letterTemplateProvider = null!;
        protected Mock<IDocumentLibraryService> documentLibraryService = null!;
        protected Mock<ITemplateTokenReplacer> templateReplacer = null!;
        protected Mock<IUserInputDraftStore> draftStore = null!;

        protected CopyrightCadController controller = null!;

        [SetUp]
        public void BaseSetUp()
        {
            copyrightService = new Mock<ICopyrightService>();
            pdfService = new Mock<IPdfLetterService>();
            letterTemplateProvider = new Mock<ILetterTemplateProvider>();
            documentLibraryService = new Mock<IDocumentLibraryService>();
            templateReplacer = new Mock<ITemplateTokenReplacer>();
            draftStore = new Mock<IUserInputDraftStore>();

            controller = new CopyrightCadController(
                copyrightService.Object,
                pdfService.Object,
                letterTemplateProvider.Object,
                documentLibraryService.Object,
                templateReplacer.Object,
                draftStore.Object);

            SetupControllerContext();
        }

        protected void SetupControllerContext()
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, UserId)
                }, "TestAuth"))
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }
    }
}
