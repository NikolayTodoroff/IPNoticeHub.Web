using FluentAssertions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightDmcaControllerTests
{
    public class RecoverCopyrightDmcaControllerTests : BaseCopyrightDmcaControllerTests
    {
        [Test]
        public async Task RecoverDmca_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var actionResult = await controller.RecoverDmca(documentId: 123);

            actionResult.Should().BeOfType<ForbidResult>();

            documentLibraryService.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    It.IsAny<int>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task RecoverDmca_ShouldReturnNotFound_WhenDocumentIsNull()
        {
            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((LegalDocument)null!);

            var actionResult = await controller.RecoverDmca(documentId: 123);

            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverDmca_ShouldReturnNotFound_WhenSourceTypeIsNotCopyright()
        {
            var document = CreateValidDmcaDocument();
            document.SourceType = DocumentSourceType.Trademark;

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var actionResult = await controller.RecoverDmca(documentId: 123);

            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverDmca_ShouldReturnNotFound_WhenTemplateTypeIsNotDmca()
        {
            var document = CreateValidDmcaDocument();
            document.TemplateType = LetterTemplateType.CeaseAndDesist;

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var actionResult = 
                await controller.RecoverDmca(documentId: 123);

            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task RecoverDmca_ShouldReturnDmcaEditView_WithMappedViewModel_WhenDocumentIsValid()
        {
            var document = CreateValidDmcaDocument();

            documentLibraryService.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    123, 
                    TestUserId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var actionResult = 
                await controller.RecoverDmca(documentId: 123);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaEdit");
            viewResult.Model.Should().BeOfType<DmcaViewModel>();

            var viewModel = (DmcaViewModel)viewResult.Model!;

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

        private static LegalDocument CreateValidDmcaDocument()
        {
            return new LegalDocument
            {
                LegalDocumentId = 123,
                ApplicationUserId = "any-user",
                RelatedPublicId = Guid.NewGuid(),
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.Dmca,

                DocumentTitle = "Saved DMCA",
                IpTitle = "DB Work Title",
                RegistrationNumber = "DB-REG-123",

                SenderName = "John Doe",
                SenderAddress = "1 Main St",
                SenderEmail = "john@example.com",

                RecipientName = "Platform Legal",
                RecipientAddress = "2 High St",
                RecipientEmail = "legal@example.com",

                InfringingUrl = "https://example.com/infringing",
                AdditionalFacts = "Some facts",
                BodyTemplate = "Final body"
            };
        }
    }
}
