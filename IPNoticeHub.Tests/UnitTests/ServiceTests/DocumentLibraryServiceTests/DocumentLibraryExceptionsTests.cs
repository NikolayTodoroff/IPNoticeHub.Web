using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using static IPNoticeHub.Shared.Constants.DateTimeFormats;
using IPNoticeHub.Application.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Application.Services.DocumentLibraryService.Implementations;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using Moq;
using NUnit.Framework;
using System.Globalization;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.DocumentLibraryServiceTests
{
    public class DocumentLibraryExceptionsTests
    {
        [Test]
        public async Task SaveDocumentAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var dto = new DocumentCreateDto
            {
                RelatedPublicId = Guid.NewGuid(),
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Some title",
                IpTitle = "Nike",
                RegistrationNumber = "54321",
                SenderName = "Alice",
                SenderAddress = "Sender Street",
                RecipientName = "Bob Inc.",
                RecipientAddress = "Receiver Ave",
                LetterDate = new DateTime(
                    2025, 12, 8, 0, 0, 0, DateTimeKind.Utc),
                BodyTemplate = "Hello {{RecipientName}}"
            };

            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService = new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull = async () => await sut.SaveDocumentAsync(
                null!,
                dto,
                CancellationToken.None);

            var actWhitespace = async () => await sut.SaveDocumentAsync(
                "   ",
                dto,
                CancellationToken.None);

            var ex1 = await actNull.
                Should().ThrowAsync<ArgumentException>();

            ex1.Which.ParamName.Should().Be("userId");

            var ex2 = await actWhitespace.
                Should().ThrowAsync<ArgumentException>();

            ex2.Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.AddAsync(
                It.IsAny<LegalDocument>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveDocumentAsync_ThrowsArgumentException_WhenBodyTemplateIsNullOrWhitespace()
        {
            var userId = "user-123";

            var dtoNull = new DocumentCreateDto
            {
                RelatedPublicId = Guid.NewGuid(),
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Some title",
                IpTitle = "Nike",
                RegistrationNumber = "54321",
                SenderName = "Alice",
                SenderAddress = "Sender Street",
                RecipientName = "Bob Inc.",
                RecipientAddress = "Receiver Ave",
                LetterDate = new DateTime(
                    2025, 12, 8, 0, 0, 0, DateTimeKind.Utc),
                BodyTemplate = null
            };

            var dtoWhitespace = new DocumentCreateDto
            {
                RelatedPublicId = dtoNull.RelatedPublicId,
                SourceType = dtoNull.SourceType,
                TemplateType = dtoNull.TemplateType,
                DocumentTitle = dtoNull.DocumentTitle,
                IpTitle = dtoNull.IpTitle,
                RegistrationNumber = dtoNull.RegistrationNumber,
                SenderName = dtoNull.SenderName,
                SenderAddress = dtoNull.SenderAddress,
                RecipientName = dtoNull.RecipientName,
                RecipientAddress = dtoNull.RecipientAddress,
                LetterDate = dtoNull.LetterDate,
                BodyTemplate = "   "
            };

            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull = async () => await sut.SaveDocumentAsync(
                userId,
                dtoNull,
                CancellationToken.None);

            var actWhitespace = async () => await sut.SaveDocumentAsync(
                userId,
                dtoWhitespace,
                CancellationToken.None);

            var ex1 = await actNull.
                Should().ThrowAsync<ArgumentException>();

            ex1.Which.ParamName.Should().Be("BodyTemplate");

            var ex2 = await actWhitespace.
                Should().ThrowAsync<ArgumentException>();

            ex2.Which.ParamName.Should().Be("BodyTemplate");

            repository.Verify(r => r.AddAsync(
                It.IsAny<LegalDocument>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetUserDocumentsAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.GetUserDocumentsAsync(
                    null!,
                    null,
                    null, CancellationToken.None);

            var actWs =
                async () => await sut.GetUserDocumentsAsync(
                    "   ",
                    null,
                    null,
                    CancellationToken.None);

            (await actNull.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            (await actWs.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.GetUserDocumentsAsync(
                It.IsAny<string>(),
                It.IsAny<DocumentSourceType?>(),
                It.IsAny<LetterTemplateType?>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSingleDocumentByIdAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.GetSingleDocumentByIdAsync(
                    1,
                    null!,
                    CancellationToken.None);

            var actWs =
                async () => await sut.GetSingleDocumentByIdAsync(
                    1,
                    "  ",
                    CancellationToken.None);

            (await actNull.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            (await actWs.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.GetDocumentByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSingleDocumentByIdAsync_ThrowsInvalidOperationException_WhenDocumentNotFound()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            repository.Setup(
                r => r.GetDocumentByIdAsync(
                    5,
                    "user-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((LegalDocument?)null);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var act =
                async () => await sut.GetSingleDocumentByIdAsync(
                    5,
                    "user-1",
                    CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("Document not found or access denied.");

            repository.VerifyAll();
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.RestoreSavedDocumentAsync(
                    1, null!, CancellationToken.None);

            var actWhitespace =
                async () => await sut.RestoreSavedDocumentAsync(
                    1, "  ", CancellationToken.None);

            var ex1 = await actNull.
                Should().ThrowAsync<ArgumentException>();

            ex1.Which.ParamName.Should().Be("userId");

            var ex2 = await actWhitespace.
                Should().ThrowAsync<ArgumentException>();

            ex2.Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.GetDocumentByIdAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);

            pdfService.Verify(
                p => p.GenerateFromSavedDocumentAsync(
                    It.IsAny<LegalDocument>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task RenameDocumentAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.RenameDocumentAsync(
                    1,
                    null!,
                    "New Title",
                    CancellationToken.None);

            var actWs =
                async () => await sut.RenameDocumentAsync(
                    1,
                    "   ",
                    "New Title",
                    CancellationToken.None);

            (await actNull.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            (await actWs.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.RenameAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task RenameDocumentAsync_ThrowsArgumentOutOfRangeException_WhenDocumentIdIsNotPositive()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actZero =
                async () => await sut.RenameDocumentAsync(
                    0,
                    "user-1",
                    "New Title",
                    CancellationToken.None);

            var actNegative =
                async () => await sut.RenameDocumentAsync(
                    -1,
                    "user-1",
                    "New Title",
                    CancellationToken.None);

            (await actZero.Should().ThrowAsync<ArgumentOutOfRangeException>())
                .Which.ParamName.Should().Be("documentId");

            (await actNegative.Should().ThrowAsync<ArgumentOutOfRangeException>())
                .Which.ParamName.Should().Be("documentId");

            repository.Verify(r => r.RenameAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task RenameDocumentAsync_ThrowsArgumentException_WhenNewTitleIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.RenameDocumentAsync(
                    1,
                    "user-1",
                    null!,
                    CancellationToken.None);

            var actWs =
                async () => await sut.RenameDocumentAsync(
                    1,
                    "user-1",
                    "   ",
                    CancellationToken.None);

            (await actNull.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("newTitle");

            (await actWs.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("newTitle");

            repository.Verify(r => r.RenameAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteDocumentAsync_ThrowsArgumentException_WhenUserIdIsNullOrWhitespace()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var actNull =
                async () => await sut.DeleteDocumentAsync(
                    1,
                    null!,
                    CancellationToken.None);

            var actWs =
                async () => await sut.DeleteDocumentAsync(
                    1,
                    "   ",
                    CancellationToken.None);

            (await actNull.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            (await actWs.Should().ThrowAsync<ArgumentException>())
                .Which.ParamName.Should().Be("userId");

            repository.Verify(r => r.SoftDeleteAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_ThrowsInvalidOperationException_WhenPdfServiceThrowsNotSupportedException()
        {
            var repository =
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService =
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var document = new LegalDocument
            {
                LegalDocumentId = 99,
                DocumentTitle = "Some Title",
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                CreatedOn = new DateTime(
                    2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),
            };

            repository.Setup(
                r => r.GetDocumentByIdAsync(
                    99,
                    "user-1",
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(document);

            pdfService.Setup(
                p => p.GenerateFromSavedDocumentAsync(
                    document,
                    It.IsAny<CancellationToken>())).
                    ThrowsAsync(new NotSupportedException("nope"));

            var sut = new DocumentLibraryService(
                repository.Object,
                pdfService.Object);

            var act =
                async () => await sut.RestoreSavedDocumentAsync(
                    99,
                    "user-1",
                    CancellationToken.None);

            var ex = await act.
                Should().ThrowAsync<InvalidOperationException>();

            ex.Which.Message.Should().Contain("Unsupported document combination:");

            ex.Which.Message.Should().Contain(
                $"{document.SourceType}/{document.TemplateType}");

            ex.Which.InnerException.Should().BeOfType<NotSupportedException>();

            repository.Verify(
                r => r.GetDocumentByIdAsync(
                    99,
                    "user-1",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            pdfService.Verify(
                p => p.GenerateFromSavedDocumentAsync(
                    document,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RestoreSavedDocumentAsync_UsesFallbackTitle_WhenSanitizedDocumentTitleIsWhitespace()
        {
            var repository = 
                new Mock<IDocumentLibraryRepository>(MockBehavior.Strict);

            var pdfService = 
                new Mock<IPdfLetterService>(MockBehavior.Strict);

            var created = new DateTime(
                2025, 12, 14, 10, 30, 0, DateTimeKind.Utc);

            var invalidTitle = new string(Path.GetInvalidFileNameChars());

            var document = new LegalDocument
            {
                LegalDocumentId = 42,
                DocumentTitle = invalidTitle,
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                CreatedOn = created
            };

            var pdfBytes = new byte[] { 1, 2, 3 };

            repository.Setup(r => r.GetDocumentByIdAsync(42, "user-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            pdfService.Setup(
                p => p.GenerateFromSavedDocumentAsync(
                    document, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(pdfBytes);

            var sut = new DocumentLibraryService(
                repository.Object, 
                pdfService.Object);

            var result = await sut.RestoreSavedDocumentAsync(
                42, 
                "user-1", 
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Value.Pdf.Should().Equal(pdfBytes);

            result.Value.fileName.Should()
                .Be($"IP Infringement Notice-" +
                $"{created.ToString(DefaultDateTimeFormat.DateTimeFormat, 
                CultureInfo.InvariantCulture)}.pdf");

            repository.Verify(
                r => r.GetDocumentByIdAsync(
                    42, 
                    "user-1", 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            pdfService.Verify(
                p => p.GenerateFromSavedDocumentAsync(
                    document, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}

