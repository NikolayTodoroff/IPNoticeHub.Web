using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkCadControllerTests
{
    public class TrademarkCadControllerTests
    {
        private Mock<ITrademarkSearchService> trademarkSearchService = null!;
        private Mock<ITrademarkCollectionService> trademarkCollectionService = null!;
        private Mock<IPdfLetterService> pdfService = null!;
        private Mock<ILetterTemplateProvider> letterTemplateProvider = null!;
        private Mock<IDocumentLibraryService> documentLibraryService = null!;
        private Mock<ITemplateTokenReplacer> templateReplacer = null!;
        private TrademarkCadController controller = null!;
        private const string TestUserId = "test-user-id";

        [SetUp]
        public void SetUp()
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

        private void SetupControllerContext()
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, TestUserId)
                }, "TestAuth"))
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnViewWithViewModel_WhenTrademarkExistsAndInCollection()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456",
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    "Test Template Body"
                ));

            trademarkSearchService.
                Setup(s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(trademarkDto);

            trademarkCollectionService.
                Setup(s => s.IsInCollectionAsync(
                    TestUserId, 
                    trademarkDto.Id, 
                    false, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<CeaseDesistViewModel>();

            var model = viewResult.Model as CeaseDesistViewModel;

            model!.PublicId.Should().Be(publicId);
            model.WorkTitle.Should().Be("Test Trademark");
            model.RegistrationNumber.Should().Be("REG123456");
            model.BodyTemplate.Should().Be("Test Template Body");

            controller.ViewData["ShowAdditionalFacts"].Should().Be(true);
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnNotFound_WhenTrademarkDoesNotExist()
        {
            var publicId = Guid.NewGuid();

            trademarkSearchService.
                Setup(s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync((TrademarkDetailsDto?)null);

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnNotFound_WhenTrademarkNotInCollection()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.
                Setup(s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(trademarkDto);

            trademarkCollectionService.
                Setup(s => s.IsInCollectionAsync(
                    TestUserId, 
                    trademarkDto.Id,
                    false, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var publicId = Guid.NewGuid();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldUseEmptyTemplate_WhenTemplateNotFound()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.
                Setup(s => s.GetDetailsAsync(publicId, It.IsAny<CancellationToken>())).
                ReturnsAsync(trademarkDto);

            trademarkCollectionService.
                Setup(s => s.IsInCollectionAsync(
                    TestUserId,
                    trademarkDto.Id,
                    false,
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Trademark")).
                Returns((LetterTemplatePreset?)null);

            var result = await controller.CeaseDesist(publicId);

            var viewResult = result as ViewResult;
            var model = viewResult.Model as CeaseDesistViewModel;
            model!.BodyTemplate.Should().BeEmpty();
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnFileResult_WhenModelStateIsValid()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                SenderName = "John Doe",
                RecipientName = "Jane Smith"
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            pdfService.
                Setup(s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(pdfBytes);

            var result = await controller.CeaseDesist(publicId, viewModel);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;

            fileResult!.ContentType.Should().Be("application/pdf");
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);
            fileResult.FileDownloadName.Should().Contain("CeaseDesist-Test Work");
            fileResult.FileDownloadName.Should().Contain(".pdf");
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = await controller.CeaseDesist(publicId, viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldCallPdfServiceWithCorrectInput()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                SenderName = "John Doe",
                RecipientName = "Jane Smith"
            };

            var pdfBytes = new byte[] { 1, 2, 3 };

            pdfService.
                Setup(s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(pdfBytes);

            await controller.CeaseDesist(publicId, viewModel);

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldReplaceTokens_WhenBodyContainsPlaceholders()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "Dear {{RecipientName}}, regarding {{WorkTitle}}"
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist", 
                    "Dear {{RecipientName}}, regarding {{WorkTitle}}"));

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                Returns("Replaced Template");

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.ViewName.Should().Be("CeaseDesistPreview");

            var model = viewResult.Model as CeaseDesistViewModel;
            model!.BodyTemplate.Should().Be("Replaced Template");

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()),
                Times.Once);
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldNotReplaceTokens_WhenBodyHasNoPlaceholders()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "This is a complete template with no placeholders"
            };

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as CeaseDesistViewModel;

            model!.BodyTemplate.Should().Be(
                "This is a complete template with no placeholders");

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(), 
                    It.IsAny<Dictionary<string, string>>()),
                Times.Never);
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldReplaceTokens_WhenBodyIsEmpty()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = string.Empty
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    ""));

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                Returns("Replaced Template");

            var result = controller.CeaseDesistPreview(viewModel);

            var viewResult = result as ViewResult;
            var model = viewResult!.Model as CeaseDesistViewModel;

            model!.BodyTemplate.Should().Be("Replaced Template");
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldRedirectToPreview_WhenModelStateIsValid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "{{WorkTitle}}"
            };

            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.
                Setup(s => s.GetDetailsAsync(
                    viewModel.PublicId,
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(trademarkDto);

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    "{{WorkTitle}}"));

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                Returns("Replaced Template");

            var result = await controller.CeaseDesistPreview(viewModel, CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = result as RedirectToActionResult;
            redirectResult!.Should().BeOfType<RedirectToActionResult>();
            redirectResult.ActionName.Should().Be(nameof(controller.CeaseDesistPreview));
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistPreview");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldFetchTrademarkDetails_WhenAvailable()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Original Work",
                BodyTemplate = "Body {{Wordmark}}"
            };

            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Updated Trademark",
                RegistrationNumber = "REG999"
            };

            trademarkSearchService
                .Setup(s => s.GetDetailsAsync(
                    viewModel.PublicId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(trademarkDto);

            var result = 
                await controller.CeaseDesistPreview(viewModel, CancellationToken.None);

            trademarkSearchService.Verify(
                s => s.GetDetailsAsync(
                    viewModel.PublicId, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnView_WithViewModel()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = controller.CeaseDesistEdit(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistEdit(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldSaveDocument_WhenCommandIsSave()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                SenderName = "John Doe"
            };

            documentLibraryService
                .Setup(s => s.SaveDocumentAsync(
                    TestUserId, 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;

            redirectResult!.ActionName.Should().Be(nameof(controller.CeaseDesistEdit));

            controller.TempData["SuccessMessage"].Should()
                .Be("Your Cease & Desist letter was successfully saved to your library.");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    TestUserId, 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldRedirectToMyCollection_WhenCommandIsDone()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = await controller.CeaseDesistEdit(viewModel, "done");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;

            redirectResult!.ActionName.Should().Be("MyCollection");
            redirectResult.ControllerName.Should().Be("Trademarks");
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenCommandIsUnknown()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = 
                await controller.CeaseDesistEdit(viewModel, "unknown");

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldNotSaveDocument_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            await controller.CeaseDesistEdit(viewModel, "save");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnView_WhenDocumentExists()
        {
            var documentId = 1;
            var document = new LegalDocument
            {
                LegalDocumentId = documentId,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Test Document",
                BodyTemplate = "Test Content"
            };

            documentLibraryService.
                Setup(s => s.GetSingleDocumentByIdAsync(
                    documentId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(document);

            var result = await controller.RecoverCeaseDesist(documentId);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().BeOfType<CeaseDesistViewModel>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenDocumentDoesNotExist()
        {
            var documentId = 1;

            documentLibraryService.
                Setup(s => s.GetSingleDocumentByIdAsync(
                    documentId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((LegalDocument)null!);

            var result = await controller.RecoverCeaseDesist(documentId);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenSourceTypeIsNotTrademark()
        {
            var documentId = 1;
            var document = new LegalDocument
            {
                LegalDocumentId = documentId,
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.CeaseAndDesist
            };

            documentLibraryService.
                Setup(s => s.GetSingleDocumentByIdAsync(
                    documentId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var result = await controller.RecoverCeaseDesist(documentId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenTemplateTypeIsNotCeaseAndDesist()
        {
            var existingDocId = 1;
            var fakeDocId = 1;

            var document = new LegalDocument
            {
                LegalDocumentId = existingDocId,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist
            };

            documentLibraryService.
                Setup(s => s.GetSingleDocumentByIdAsync(
                    existingDocId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(document);

            var result = await controller.RecoverCeaseDesist(fakeDocId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var documentId = 1;

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.RecoverCeaseDesist(documentId);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldCallDocumentLibraryService_WithCorrectParameters()
        {
            var documentId = 42;

            documentLibraryService.
                Setup(s => s.GetSingleDocumentByIdAsync(
                    documentId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync((LegalDocument)null!);

            await controller.RecoverCeaseDesist(documentId);

            documentLibraryService.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    documentId, 
                    TestUserId, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}