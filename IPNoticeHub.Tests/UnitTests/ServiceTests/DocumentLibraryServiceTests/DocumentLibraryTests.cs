using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions;
using IPNoticeHub.Application.DocumentLibrary.DTOs;
using IPNoticeHub.Application.DocumentLibrary.Implementations;
using IPNoticeHub.Application.PdfGeneration.Abstractions;
using Moq;
using NUnit.Framework;

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
                LetterDate = new DateTime(2025, 
                12, 
                8, 
                0, 
                0, 
                0, 
                DateTimeKind.Utc),
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
                .Callback<LegalDocument, CancellationToken>((
                    ld, _) => entity = ld)
                .ReturnsAsync(42);

            var pdfService = new Mock<IPdfService>(MockBehavior.Loose);

            var logger = 
                new Mock<DocumentLibraryService>(MockBehavior.Loose);

            var sut = new DocumentLibraryService(
                repository.Object, 
                pdfService.Object);


            var id = await sut.SaveDocumentAsync(
                userId, 
                dto, 
                CancellationToken.None);

            id.Should().
                Be(42);

            entity.Should().
                NotBeNull();

            entity!.UserId.Should().
                Be(userId);

            entity.RelatedPublicId.Should().
                Be(dto.RelatedPublicId);

            entity.SourceType.Should().
                Be(dto.SourceType);

            entity.TemplateType.Should().
                Be(dto.TemplateType);

            entity.DocumentTitle.Should().
                NotBeNullOrWhiteSpace();

            entity.DocumentTitle.Should().
                Contain("Nike");

            entity.SenderName.Should().
                Be(dto.SenderName);

            entity.RecipientName.Should().
                Be(dto.RecipientName);

            repository.Verify(r => r.AddAsync(
                It.IsAny<LegalDocument>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task RestoreDocumentSnapshotAsync_ReturnsPdfAndFileName()
        {
            var userId = "user-123";
            var documentId = 7;

            var document = new LegalDocument
            {
                Id = documentId,
                UserId = userId,
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
                LetterDate = new DateTime(
                    2025, 
                    12, 
                    8, 
                    0, 
                    0, 
                    0, 
                    DateTimeKind.Utc),
                AdditionalFacts = "Extra facts",
                BodyTemplate = "Dear {{RecipientName}},\nThis concerns {{WorkTitle}} " +
                "{{RegistrationNumber}}.\nSincerely,\n{{SenderName}}"
            };

            var repository = 
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            repository.Setup(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            var generatedPdf = new byte[] { 1, 2, 3, 4 };

            CeaseDesistInput? capturedInput = null;

            var pdfService = new Mock<IPdfService>(MockBehavior.Strict);

            pdfService
                .Setup(p => p.GenerateTrademarkCeaseDesistAsync(
                    It.IsAny<CeaseDesistInput>(),
                    It.IsAny<CancellationToken>()))
                .Callback<
                    CeaseDesistInput, 
                    CancellationToken>((
                        input, _) => capturedInput = input)
                .ReturnsAsync(generatedPdf);

            var logger = 
                new Mock<DocumentLibraryService>(MockBehavior.Loose);

            var documentService = new DocumentLibraryService(
                repository.Object, 
                pdfService.Object);

            var result = await documentService.RestoreDocumentSnapshotAsync(
                documentId, 
                userId, 
                CancellationToken.None);

            result.Should().NotBeNull();

            var (fileName, pdf) = result!.Value;

            pdf.Should().
                BeSameAs(generatedPdf);

            fileName.Should().
                EndWith(".pdf");

            fileName.Should().
                Contain("Cease");

            fileName.Should().
                Contain("Nike");

            capturedInput.Should().NotBeNull();

            capturedInput!.SenderName.Should().
                Be(document.SenderName);

            capturedInput.SenderAddress.Should().
                Be(document.SenderAddress);

            capturedInput.RecipientName.Should().
                Be(document.RecipientName);

            capturedInput.RecipientAddress.Should().
                Be(document.RecipientAddress);

            capturedInput.WorkTitle.Should().
                Be(document.IpTitle);

            capturedInput.RegistrationNumber.Should().
                Be(document.RegistrationNumber);

            capturedInput.Date.Should().
                Be(document.LetterDate);

            capturedInput.AdditionalFacts.Should().
                Be(document.AdditionalFacts);

            capturedInput.BodyTemplate.Should().
                Be(document.BodyTemplate);

            repository.Verify(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            pdfService.Verify(p => p.GenerateTrademarkCeaseDesistAsync(
                    It.IsAny<CeaseDesistInput>(),
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
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((LegalDocument?)null);

            var pdfService = new Mock<IPdfService>(MockBehavior.Loose);

            var documentService = 
                new Mock<DocumentLibraryService>(MockBehavior.Loose);

            var sut = new DocumentLibraryService(
                repo.Object, 
                pdfService.Object);

            var result = await sut.RestoreDocumentSnapshotAsync(
                documentId, 
                userId, 
                CancellationToken.None);

            result.Should().BeNull();

            pdfService.Verify(p => p.GenerateTrademarkCeaseDesistAsync(
                    It.IsAny<CeaseDesistInput>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            pdfService.Verify(p => p.GenerateCopyrightDMCAAsync(
                    It.IsAny<DMCAInput>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
