using FluentAssertions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Infrastructure.Rendering;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RenderingTests
{
    public class QuestPdfGeneratorTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        [Test]
        public void GenerateDocument_CallsTokenReplacer_AndReturnsPdfBytes()
        {
            var templateReplacer = 
                new Mock<ITemplateTokenReplacer>(MockBehavior.Strict);

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    It.IsAny<string>(), 
                    It.IsAny<IReadOnlyDictionary<string, string>>())).
                Returns("Hello world");

            var pdfGenerator = 
                new QuestPdfGenerator(templateReplacer.Object);

            var dto = new PdfLetterDto
            {
                SenderName = "Nikolay",
                BodyTemplate = "Ignored because replacer returns output",
                Tokens = new Dictionary<string, string> { ["SenderName"] = "Nikolay" },
                DateUtc = DateTime.UtcNow,
                RecipientName = "Recipient",
                RecipientAddress = "Addr",
            };

            var bytes = pdfGenerator.GenerateDocument(dto);

            bytes.Should().NotBeNull();
            bytes.Length.Should().BeGreaterThan(0);

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    dto.BodyTemplate, dto.Tokens), 
                Times.Once);
            
            templateReplacer.VerifyNoOtherCalls();
        }

        [Test]
        public void GenerateDocument_WhenSenderAddressIsNull_DoesNotThrow_AndReturnsPdfBytes()
        {
            var replacer = 
                new Mock<ITemplateTokenReplacer>();

            replacer.Setup(r => r.ReplaceTemplate(
                It.IsAny<string>(), 
                It.IsAny<IReadOnlyDictionary<string, string>>())).
                Returns("Body");

            var sut = new QuestPdfGenerator(replacer.Object);

            var dto = new PdfLetterDto
            {
                SenderName = "Sender",
                SenderAddress = null,
                BodyTemplate = "Body",
                Tokens = new Dictionary<string, string>(),
                DateUtc = DateTime.UtcNow,
            };

            var act = () => sut.GenerateDocument(dto);

            act.Should().NotThrow();
            act().Length.Should().BeGreaterThan(0);
        }

        [Test]
        public void GenerateDocument_WhenRecipientNameAndAddressEmpty_DoesNotThrow_AndReturnsPdfBytes()
        {
            var replacer = new Mock<ITemplateTokenReplacer>();
            replacer.Setup(r => r.ReplaceTemplate(
                It.IsAny<string>(), 
                It.IsAny<IReadOnlyDictionary<string, string>>())).
                Returns("Body");

            var sut = new QuestPdfGenerator(replacer.Object);

            var dto = new PdfLetterDto
            {
                SenderName = "Sender",
                BodyTemplate = "Body",
                Tokens = new Dictionary<string, string>(),
                DateUtc = DateTime.UtcNow,
                RecipientName = "",
                RecipientAddress = "   "
            };

            var bytes = sut.GenerateDocument(dto);

            bytes.Length.Should().BeGreaterThan(0);
        }
    }
}
