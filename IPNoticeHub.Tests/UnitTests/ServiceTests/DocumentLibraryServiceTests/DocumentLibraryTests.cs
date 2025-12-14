using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Application.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Services.DocumentLibraryService.Implementations;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.DocumentLibraryServiceTests
{
    [TestFixture]
    public class DocumentLibraryTests
    {
        [Test]
        public async Task SaveDocumentAsync_GeneratesDefaultTitle_WhenTitleMissing()
        {
            var userId = "user-123";

            var dto = new DocumentCreateDto
            {
                RelatedPublicId = Guid.NewGuid(),
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = null,
                IpTitle = "Nike",
                RegistrationNumber = "54321",
                SenderName = "Alice",
                SenderAddress = "Sender Street",
                RecipientName = "Bob Inc.",
                RecipientAddress = "Receiver Ave",
                LetterDate = 
                new DateTime(2025,12,8,0,0,0,DateTimeKind.Utc),
                BodyTemplate = "Hello {{RecipientName}}, " +
                "this concerns {{WorkTitle}} {{RegistrationNumber}}."
            };

            var entity = (LegalDocument?)null;

            var repository = 
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            repository.Setup(
                r => r.AddAsync(
                    It.IsAny<LegalDocument>(), 
                    It.IsAny<CancellationToken>()))
                .Callback<LegalDocument, CancellationToken>((ld, _) => 
                entity = ld).
                ReturnsAsync(42);

            var pdfService = 
                new Mock<IPdfLetterService>(MockBehavior.Loose);

            var logger = 
                new Mock<DocumentLibraryService>(MockBehavior.Loose);

            var service = 
                new DocumentLibraryService(repository.Object,pdfService.Object);

            var id = await service.SaveDocumentAsync(userId,dto,CancellationToken.None);

            id.Should().Be(42);
            entity.Should().NotBeNull();
            entity!.ApplicationUserId.Should().Be(userId);
            entity.RelatedPublicId.Should().Be(dto.RelatedPublicId);
            entity.SourceType.Should().Be(dto.SourceType);
            entity.TemplateType.Should().Be(dto.TemplateType);
            entity.DocumentTitle.Should().NotBeNullOrWhiteSpace();

            entity.DocumentTitle.Should().Contain("Nike");
            entity.SenderName.Should().Be(dto.SenderName);
            entity.RecipientName.Should().Be(dto.RecipientName);

            repository.Verify(r => r.AddAsync(
                It.IsAny<LegalDocument>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_ReturnsPdfAndFileName()
        {
            var userId = "user-123";
            var documentId = 7;

            var document = new LegalDocument
            {
                LegalDocumentId = documentId,
                ApplicationUserId = userId,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Cease and Desist – Nike 54321",
                IpTitle = "Nike",
                RegistrationNumber = "54321",
                SenderName = "Test Sender",
                SenderAddress = "Test Sender Address",
                SenderEmail = "sender@example.com",
                RecipientName = "Test Recipient",
                RecipientAddress = "Test Recipient Address",
                RecipientEmail = "recipient@example.com",
                LetterDate = 
                new DateTime(2025,12,8,0,0,0,DateTimeKind.Utc),
                AdditionalFacts = "Extra facts",
                BodyTemplate = "Dear {{RecipientName}},\nThis concerns {{WorkTitle}} " +
                "{{RegistrationNumber}}.\nSincerely,\n{{SenderName}}"
            };

            var repository = 
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            repository.Setup(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(document);

            var generatedPdf = new byte[] { 1, 2, 3, 4 };

            LegalDocument? capturedDocument = null;

            var pdfService = new Mock<IPdfLetterService>(MockBehavior.Strict);

            pdfService.Setup(p => p.GenerateFromSavedDocumentAsync(
                    It.IsAny<LegalDocument>(),
                    It.IsAny<CancellationToken>())).
                Callback<LegalDocument, CancellationToken>((doc, _) 
                    => capturedDocument = doc).
                ReturnsAsync(generatedPdf);

            var documentService = new DocumentLibraryService(
                repository.Object, 
                pdfService.Object);

            var result = await documentService.RestoreSavedDocumentAsync(
                documentId, 
                userId, 
                CancellationToken.None);

            result.Should().NotBeNull();

            var (fileName, pdf) = result!.Value;

            pdf.Should().BeSameAs(generatedPdf);
            fileName.Should().EndWith(".pdf");
            fileName.Should().Contain("Cease");
            fileName.Should().Contain("Nike");
            
            capturedDocument.Should().NotBeNull();
            capturedDocument.LetterDate.Should().Be(document.LetterDate);
            capturedDocument.IpTitle.Should().Be(document.IpTitle);
            capturedDocument.RegistrationNumber.Should().Be(document.RegistrationNumber);
            capturedDocument.SenderName.Should().Be(document.SenderName);
            capturedDocument.SenderAddress.Should().Be(document.SenderAddress);
            capturedDocument.RecipientName.Should().Be(document.RecipientName);
            capturedDocument.RecipientAddress.Should().Be(document.RecipientAddress);
            capturedDocument.AdditionalFacts.Should().Be(document.AdditionalFacts);
            capturedDocument.BodyTemplate.Should().Be(document.BodyTemplate);

            repository.Verify(r => r.GetDocumentByIdAsync(
                documentId,
                userId,
                It.IsAny<CancellationToken>()),
                Times.Once);

            pdfService.Verify(p => p.GenerateFromSavedDocumentAsync(
                    It.IsAny<LegalDocument>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RestoreDocumentSnapshotAsync_ReturnsNull_WhenDocumentNotFound()
        {
            var userId = "user-123";
            var documentId = 99;

            var repo = 
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            repo.Setup(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((LegalDocument?)null);

            var pdfService = new Mock<IPdfLetterService>(MockBehavior.Loose);

            var documentService = 
                new Mock<DocumentLibraryService>(MockBehavior.Loose);

            var sut = new DocumentLibraryService(
                repo.Object, 
                pdfService.Object);

            var result = await sut.RestoreSavedDocumentAsync(
                documentId, 
                userId, 
                CancellationToken.None);

            result.Should().BeNull();

            pdfService.Verify(p => p.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            pdfService.Verify(p => p.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
