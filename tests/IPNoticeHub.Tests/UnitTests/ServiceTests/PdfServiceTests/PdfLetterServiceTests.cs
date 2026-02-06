using FluentAssertions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Implementations;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using Moq;
using NUnit.Framework;
using QuestPDF.Infrastructure;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.PdfServiceTests
{
    public class PdfLetterServiceTests : PdfLetterServiceBase
    {
        [Test]
        public void GeneratesNonEmptyPdf_ForSimpleTemplate()
        {
            letterAssembler.Setup(x => 
            x.RebuildLetterInput(It.IsAny<LetterInputDto>())).
            Returns(new PdfLetterDto());

            pdfGenerator.Setup(
                x => x.GenerateDocument(It.IsAny<PdfLetterDto>())).
                Returns(() => {return new byte[1200];});

            var inputDto = new LetterInputDto
            {
                SenderName = "Alice",
                SenderAddress = "123 Street",
                RecipientName = "Bob Inc.",
                RecipientAddress = "456 Ave",
                LetterDateUtc = DateTime.UtcNow,
                WorkTitle = "My Work",
                RegistrationNumber = "REG123",
                AdditionalFacts = "Details here.",
                BodyTemplate = "Hello {{RecipientName}}, {{WorkTitle}} {{RegistrationNumber}}." +
                " Regards, {{SenderName}}"
            };

            var pdf = service.GenerateFromInputAsync(inputDto).Result;
            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void DMCA_GeneratePdf_WithMinimalRequiredFields()
        {
            letterAssembler.Setup(x =>
            x.RebuildLetterInput(It.IsAny<LetterInputDto>())).
            Returns(new PdfLetterDto());

            pdfGenerator.Setup(
                x => x.GenerateDocument(It.IsAny<PdfLetterDto>())).
                Returns(() => { return new byte[1200]; });

            var inputDto = new LetterInputDto
            {
                SenderName = "Carol",
                SenderEmail = "carol@example.com",
                SenderAddress = "742 Evergreen",
                RecipientName = "YouTube Legal",
                RecipientEmail = string.Empty,
                RecipientAddress = string.Empty,
                LetterDateUtc = DateTime.UtcNow,
                WorkTitle = "Song of Silence",
                RegistrationNumber = "TX-001122",
                YearOfCreation = null,
                DateOfPublication = null,
                NationOfFirstPublication = null,
                InfringingUrl = "https://example.com/bad",
                GoodFaithStatement = "I have a good faith belief...",
                BodyTemplate = "Dear {{RecipientName}},\n\nI, {{SenderName}} ({{SenderEmail}}), " +
                "submit a DMCA notice for \"{{WorkTitle}}\" ({{RegistrationNumber}}). " +
                "The infringing URL is {{InfringingUrl}}.\n\n{{GoodFaithStatement}}\n\n{{SenderAddress}}"
            };

            var pdf = service.GenerateFromInputAsync(inputDto).Result;
            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void GeneratePdf_WithNullAdditionalFacts_DoesNotBreakPdf()
        {
            letterAssembler.Setup(x =>
            x.RebuildLetterInput(It.IsAny<LetterInputDto>())).
            Returns(new PdfLetterDto());

            pdfGenerator.Setup(
                x => x.GenerateDocument(It.IsAny<PdfLetterDto>())).
                Returns(() => { return new byte[1200]; });

            var inputDto = new LetterInputDto
            {
                SenderName = "Alice",
                SenderAddress = "Address1",
                RecipientName = "Tom",
                RecipientAddress = "Address2",
                LetterDateUtc = DateTime.UtcNow,
                WorkTitle = "WorkTitle1",
                RegistrationNumber = "TM-111",
                AdditionalFacts = null,
                BodyTemplate = "C&D for {{WorkTitle}} ({{RegistrationNumber}}). " +
                "{{AdditionalFacts}}\nThanks, {{SenderName}}"
            };

            var pdf = service.GenerateFromInputAsync(inputDto).Result;

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public async Task GenerateFromSavedDocumentAsync_ShouldRebuildDto_GeneratePdf_AndReturnBytes()
        {
            var document = new LegalDocument();

            var rebuiltDto = new PdfLetterDto
            {
                DocumentType = "Any",
                DocumentTitle = "Title",
                WorkTitle = "Work",
                BodyTemplate = "Body"
            };

            var expectedBytes = new byte[] { 1, 2, 3, 4 };

            legalDocumentAssembler.Setup(
                x => x.RebuildFromSavedDocument(document)).
                Returns(rebuiltDto);

            pdfGenerator.Setup(
                x => x.GenerateDocument(rebuiltDto)).
                Returns(expectedBytes);

            var result = 
                await service.GenerateFromSavedDocumentAsync(
                    document, 
                    CancellationToken.None);

            result.Should().BeEquivalentTo(expectedBytes);

            legalDocumentAssembler.Verify(
                x => x.RebuildFromSavedDocument(document), 
                Times.Once);

            pdfGenerator.Verify(
                x => x.GenerateDocument(rebuiltDto), 
                Times.Once);

            legalDocumentAssembler.VerifyNoOtherCalls();
            pdfGenerator.VerifyNoOtherCalls();
            letterAssembler.VerifyNoOtherCalls();
        }
    }
}
