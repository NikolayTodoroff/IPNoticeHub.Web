using FluentAssertions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.DocumentLibraryControllerTests
{
    public class EditDocumentLibraryTests
    {
        [Test]
        public async Task Edit_WhenUserIdMissing_ReturnsForbid()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId: null);

            var result = await controller.Edit(id: 1);

            result.Should().BeOfType<ForbidResult>();

            service.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Edit_WhenDocumentNotFound_ReturnsNotFound()
        {
            var service = new Mock<IDocumentLibraryService>();
            const string userId = "user-123";
            const int documentId = 99;

            service
                .Setup(s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((LegalDocument)null!);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            var result = await controller.Edit(documentId);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Edit_TrademarkDocument_RedirectsToTrademarkCadRecoverCeaseDesist()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            const string userId = "user-123";
            const int documentId = 10;
            const int legalDocumentId = 42;

            var document = new LegalDocument
            {
                LegalDocumentId = legalDocumentId,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Trademark Document",
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "Test Address",
                RecipientName = "Test Recipient",
                RecipientAddress = "Recipient Address",
                LetterDate = DateTime.Now,
                BodyTemplate = "Template",
                CreatedOn = DateTime.Now
            };

            service.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            var result = await controller.Edit(documentId);

            var redirectResult = 
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().Be("RecoverCeaseDesist");
            redirectResult.ControllerName.Should().Be("TrademarkCad");
            redirectResult.RouteValues.Should().ContainKey("documentId");
            redirectResult.RouteValues!["documentId"].Should().Be(legalDocumentId);
        }

        [Test]
        public async Task Edit_CopyrightCeaseAndDesistDoc_RedirectsToCopyrightCadRecoverCeaseDesist()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            const string userId = "user-456";
            const int documentId = 20;
            const int legalDocumentId = 55;

            var document = new LegalDocument
            {
                LegalDocumentId = legalDocumentId,
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Copyright C&D Document",
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "Test Address",
                RecipientName = "Test Recipient",
                RecipientAddress = "Recipient Address",
                LetterDate = DateTime.Now,
                BodyTemplate = "Template",
                CreatedOn = DateTime.Now
            };

            service.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            var result = await controller.Edit(documentId);

            var redirectResult = 
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().Be("RecoverCeaseDesist");
            redirectResult.ControllerName.Should().Be("CopyrightCad");
            redirectResult.RouteValues.Should().ContainKey("documentId");
            redirectResult.RouteValues!["documentId"].Should().Be(legalDocumentId);
        }

        [Test]
        public async Task Edit_CopyrightDmcaDocument_RedirectsToCopyrightDmcaRecoverDmca()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            const string userId = "user-789";
            const int documentId = 30;
            const int legalDocumentId = 77;

            var document = new LegalDocument
            {
                LegalDocumentId = legalDocumentId,
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.Dmca,
                DocumentTitle = "DMCA Document",
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "Test Address",
                RecipientName = "Test Recipient",
                RecipientAddress = "Recipient Address",
                LetterDate = DateTime.Now,
                BodyTemplate = "Template",
                CreatedOn = DateTime.Now
            };

            service.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            var result = await controller.Edit(documentId);

            var redirectResult = 
                result.Should().BeOfType<RedirectToActionResult>().Subject;
            
            redirectResult.ActionName.Should().Be("RecoverDmca");
            redirectResult.ControllerName.Should().Be("CopyrightDmca");
            redirectResult.RouteValues.Should().ContainKey("documentId");
            redirectResult.RouteValues!["documentId"].Should().Be(legalDocumentId);
        }

        [Test]
        public async Task Edit_WhenDocumentTypeIsUnsupported_ReturnsBadRequest()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            const string userId = "user-999";
            const int documentId = 40;

            var document = new LegalDocument
            {
                LegalDocumentId = 88,
                SourceType = (DocumentSourceType)999,
                TemplateType = (LetterTemplateType)999,
                DocumentTitle = "Unsupported Document",
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "Test Address",
                RecipientName = "Test Recipient",
                RecipientAddress = "Recipient Address",
                LetterDate = DateTime.Now,
                BodyTemplate = "Template",
                CreatedOn = DateTime.Now
            };

            service.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            var result = await controller.Edit(documentId);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Unsupported document type.");
        }

        [Test]
        public async Task Edit_CallsServiceWithCorrectParameters()
        {
            var service = 
                new Mock<IDocumentLibraryService>();

            const string userId = "user-abc";
            const int documentId = 50;

            var document = new LegalDocument
            {
                LegalDocumentId = 100,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Test Document",
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "Test Address",
                RecipientName = "Test Recipient",
                RecipientAddress = "Recipient Address",
                LetterDate = DateTime.Now,
                BodyTemplate = "Template",
                CreatedOn = DateTime.Now
            };

            service.Setup(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var controller = 
                DocumentLibraryControllerFactory.CreateDocumentLibraryController(
                service,
                userId);

            await controller.Edit(documentId);

            service.Verify(
                s => s.GetSingleDocumentByIdAsync(
                    documentId,
                    userId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
    
