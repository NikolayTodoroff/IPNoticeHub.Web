using FluentAssertions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public class RecoverCopyrightCadControllerTests : BaseCopyrightCadControllerTests
    {
        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var actionResult = 
                await controller.RecoverCeaseDesist(documentId: 123);

            actionResult.Should().BeOfType<ForbidResult>();

            documentLibraryService.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    It.IsAny<int>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenDocumentIsNull()
        {
            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((LegalDocument)null!);

            var result = await controller.RecoverCeaseDesist(documentId: 123);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenSourceTypeIsNotCopyright()
        {
            var document = CreateValidCeaseDesistDocument();
            document.SourceType = DocumentSourceType.Trademark;

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var result = await controller.RecoverCeaseDesist(documentId: 123);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnNotFound_WhenTemplateTypeIsNotCeaseAndDesist()
        {
            var doc = CreateValidCeaseDesistDocument();
            doc.TemplateType = LetterTemplateType.Dmca;

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(doc);

            var result = 
                await controller.RecoverCeaseDesist(documentId: 123);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverCeaseDesist_ShouldReturnCeaseDesistEditView_WithMappedViewModel_WhenDocumentIsValid()
        {
            var document = CreateValidCeaseDesistDocument();

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var actionResult = 
                await controller.RecoverCeaseDesist(documentId: 123);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().BeOfType<CeaseDesistViewModel>();

            var viewModel = (CeaseDesistViewModel)viewResult.Model!;
            viewModel.PublicId.Should().Be(document.RelatedPublicId);
            viewModel.WorkTitle.Should().Be(document.IpTitle); 
            viewModel.RegistrationNumber.Should().Be(document.RegistrationNumber);
            viewModel.SenderName.Should().Be(document.SenderName);
            viewModel.SenderAddress.Should().Be(document.SenderAddress);
            viewModel.SenderEmail.Should().Be(document.SenderEmail);
            viewModel.RecipientName.Should().Be(document.RecipientName);
            viewModel.RecipientAddress.Should().Be(document.RecipientAddress);
            viewModel.RecipientEmail.Should().Be(document.RecipientEmail);
            viewModel.InfringingUrl.Should().Be(document.InfringingUrl);
            viewModel.AdditionalFacts.Should().Be(document.AdditionalFacts);
            viewModel.BodyTemplate.Should().Be(document.BodyTemplate);
        }

        private static LegalDocument CreateValidCeaseDesistDocument()
        {
            return new LegalDocument
            {
                LegalDocumentId = 123,
                ApplicationUserId = "any-user",
                RelatedPublicId = Guid.NewGuid(),
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.CeaseAndDesist,

                DocumentTitle = "Saved C&D",
                IpTitle = "DB Work Title",
                RegistrationNumber = "DB-REG-123",

                SenderName = "John Doe",
                SenderAddress = "1 Main St",
                SenderEmail = "john@example.com",

                RecipientName = "Jane Smith",
                RecipientAddress = "2 High St",
                RecipientEmail = "jane@example.com",

                InfringingUrl = "https://example.com/infringing",
                AdditionalFacts = "Some facts",
                BodyTemplate = "Final body"
            };
        }
    }
}
