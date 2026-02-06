using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;
using System.Globalization;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.DocumentLibraryServiceTests
{
    public class DocumentLibraryTests : DocumentLibraryBase
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

            repository.Setup(
                r => r.AddAsync(
                    It.IsAny<LegalDocument>(), 
                    It.IsAny<CancellationToken>())).
                    Callback<LegalDocument, CancellationToken>((ld, _) => entity = ld).
                ReturnsAsync(42);

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

            repository.Setup(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(document);

            var generatedPdf = new byte[] { 1, 2, 3, 4 };

            LegalDocument? capturedDocument = null;

            pdfService.Setup(p => p.GenerateFromSavedDocumentAsync(
                    It.IsAny<LegalDocument>(),
                    It.IsAny<CancellationToken>())).
                Callback<LegalDocument, CancellationToken>((doc, _) 
                    => capturedDocument = doc).
                ReturnsAsync(generatedPdf);

            var result = await service.RestoreSavedDocumentAsync(
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

            repository.Setup(r => r.GetDocumentByIdAsync(
                documentId, 
                userId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((LegalDocument?)null);

            var result = await service.RestoreSavedDocumentAsync(
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

        [Test]
        public async Task GetUserDocumentsAsync_ReturnsMappedDtos_WhenRepositoryReturnsDocuments()
        {
            var userId = "user-123";
            DocumentSourceType? sourceType = DocumentSourceType.Trademark;
            LetterTemplateType? templateType = LetterTemplateType.CeaseAndDesist;

            var created1 = new DateTime(
                2025, 12, 10, 0, 0, 0, DateTimeKind.Utc);

            var created2 = new DateTime(
                2025, 12, 11, 0, 0, 0, DateTimeKind.Utc);

            var docs = new List<LegalDocument>
    {
        new LegalDocument
        {
            LegalDocumentId = 10,
            DocumentTitle = "Doc A",
            SourceType = DocumentSourceType.Trademark,
            TemplateType = LetterTemplateType.CeaseAndDesist,
            IpTitle = "NIKE",
            RegistrationNumber = "REG-1",
            CreatedOn = created1
        },
        new LegalDocument
        {
            LegalDocumentId = 11,
            DocumentTitle = "Doc B",
            SourceType = DocumentSourceType.Copyright,
            TemplateType = LetterTemplateType.Dmca,
            IpTitle = "Some Work",
            RegistrationNumber = "REG-2",
            CreatedOn = created2
        }
    };

            repository.Setup(
                r => r.GetUserDocumentsAsync(
                    userId, 
                    sourceType, 
                    templateType, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(docs);

            var result = 
                await service.GetUserDocumentsAsync(
                userId, 
                sourceType, 
                templateType, 
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result[0].Id.Should().Be(10);
            result[0].DocumentTitle.Should().Be("Doc A");
            result[0].SourceType.Should().Be(DocumentSourceType.Trademark);
            result[0].TemplateType.Should().Be(LetterTemplateType.CeaseAndDesist);
            result[0].IpTitle.Should().Be("NIKE");
            result[0].RegistrationNumber.Should().Be("REG-1");
            result[0].CreatedOn.Should().Be(created1);

            result[1].Id.Should().Be(11);
            result[1].DocumentTitle.Should().Be("Doc B");
            result[1].SourceType.Should().Be(DocumentSourceType.Copyright);
            result[1].TemplateType.Should().Be(LetterTemplateType.Dmca);
            result[1].IpTitle.Should().Be("Some Work");
            result[1].RegistrationNumber.Should().Be("REG-2");
            result[1].CreatedOn.Should().Be(created2);

            repository.Verify(r => r.GetUserDocumentsAsync(
                userId, 
                sourceType, 
                templateType, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task RenameDocumentAsync_ReturnsTrue_AndCallsRepository()
        {
            repository.Setup(
                r => r.RenameAsync(
                    5, 
                    "user-1", 
                    "New Title", 
                    It.IsAny<CancellationToken>())).
                    Returns(Task.CompletedTask);

            var result = await service.RenameDocumentAsync(
                5, 
                "user-1", 
                "New Title", 
                CancellationToken.None);

            result.Should().BeTrue();
            repository.Verify(
                r => r.RenameAsync(
                    5, 
                    "user-1", 
                    "New Title",
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task DeleteDocumentAsync_ReturnsTrue_AndCallsRepository()
        {
            repository.Setup(
                r => r.SoftDeleteAsync(
                    7, 
                    "user-1", 
                    It.IsAny<CancellationToken>())).
                    Returns(Task.CompletedTask);

            var result = await service.DeleteDocumentAsync(
                7, 
                "user-1", 
                CancellationToken.None);

            result.Should().BeTrue();

            repository.Verify(
                r => r.SoftDeleteAsync(
                    7, 
                    "user-1", 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_ReturnsNull_WhenDocumentNotFound()
        {
            repository.Setup(
                r => r.GetDocumentByIdAsync(
                    123, 
                    "user-1", 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((LegalDocument?)null);

            var result = await service.RestoreSavedDocumentAsync(
                123, 
                "user-1", 
                CancellationToken.None);

            result.Should().BeNull();

            repository.Verify(
                r => r.GetDocumentByIdAsync(
                    123, 
                    "user-1", 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            pdfService.Verify(
                p => p.GenerateFromSavedDocumentAsync(
                    It.IsAny<LegalDocument>(), 
                    It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_ReturnsFileNameAndBytes_WhenPdfGenerationSucceeds()
        {
            var created = new DateTime(
                2025, 12, 14, 10, 30, 0, DateTimeKind.Utc);

            var doc = new LegalDocument
            {
                LegalDocumentId = 10,
                DocumentTitle = "My:Doc*Title?",
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                CreatedOn = created
            };

            var bytes = new byte[] { 1, 2, 3 };

            repository.Setup(
                r => r.GetDocumentByIdAsync(
                    10, 
                    "user-1", 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(doc);

            pdfService.Setup(
                p => p.GenerateFromSavedDocumentAsync(
                    doc, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(bytes);

            var result = 
                await service.RestoreSavedDocumentAsync(
                    10, 
                    "user-1", 
                    CancellationToken.None);

            result.Should().NotBeNull();
            result!.Value.Pdf.Should().Equal(bytes);

            var expectedSanitizedTitle = string.Join("_",
                doc.DocumentTitle.Split(
                    Path.GetInvalidFileNameChars(), 
                    StringSplitOptions.RemoveEmptyEntries));

            result.Value.fileName.Should().Be(
                $"{expectedSanitizedTitle}-{created.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}.pdf");

            repository.Verify(
                r => r.GetDocumentByIdAsync(
                    10, 
                    "user-1", 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            pdfService.Verify(
                p => p.GenerateFromSavedDocumentAsync(
                    doc, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task GetSingleDocumentByIdAsync_ReturnsDocument_WhenFound()
        {
            var document = new LegalDocument
            {
                LegalDocumentId = 7,
                DocumentTitle = "Cease & Desist",
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                SenderName = "Alice",
                RecipientName = "Bob"
            };

            repository.Setup(
                r => r.GetDocumentByIdAsync(
                    7, 
                    "user-1", 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            var result = await service.GetSingleDocumentByIdAsync(
                7, 
                "user-1", 
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeSameAs(document);

            repository.Verify(
                r => r.GetDocumentByIdAsync(
                    7, 
                    "user-1", 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
