using FluentAssertions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCadControllerTests
{
    public class RecoverTmCadControllerTests : BaseTmCadControllerTests
    {
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

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    UserId,
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

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    UserId,
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

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    UserId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var result = await controller.RecoverCeaseDesist(documentId);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenTemplateTypeIsNotCeaseAndDesist()
        {
            var existingDocId = 1;
            var fakeDocId = 2;

            var document = new LegalDocument
            {
                LegalDocumentId = existingDocId,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist
            };

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    existingDocId,
                    UserId,
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

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    UserId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((LegalDocument)null!);

            await controller.RecoverCeaseDesist(documentId);

            documentLibraryService.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    UserId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
