using FluentAssertions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Implementations;
using NUnit.Framework;
using QuestPDF.Infrastructure;

namespace IPNoticeHub.Tests.UnitTests.PdfTests
{
    public class PdfServiceTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [Test]
        public void GeneratesNonEmptyPdf_ForSimpleTemplate()
        {
            var pdfService = new PdfService();

            var ceaseDesistInput = new CeaseDesistInput(
                SenderName: "Alice",
                SenderAddress: "123 Street",
                RecipientName: "Bob Inc.",
                RecipientAddress: "456 Ave",
                Date: DateTime.UtcNow,
                WorkTitle: "My Work",
                RegistrationNumber: "REG123",
                AdditionalFacts: "Details here.",
                BodyTemplate: "Hello {{RecipientName}}, {{WorkTitle}} {{RegistrationNumber}}. Regards, {{SenderName}}"
            );

            var pdf = pdfService.GenerateTrademarkCeaseDesistAsync(ceaseDesistInput).Result;

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void DMCA_GeneratePdf_WithMinimalRequiredFields()
        {
            var pdfService = new PdfService();

            var dmcaInput = new DMCAInput(
                SenderName: "Carol",
                SenderEmail: "carol@example.com",
                SenderAddress: "742 Evergreen",
                RecipientName: "YouTube Legal",
                RecipientEmail: string.Empty,
                RecipientAddress: string.Empty,
                Date: DateTime.UtcNow,
                WorkTitle: "Song of Silence",
                RegistrationNumber: "TX-001122",
                YearOfCreation: null,
                DateOfPublication: null,
                NationOfFirstPublication: null,
                InfringingUrl: "https://example.com/bad",
                GoodFaithStatement: "I have a good faith belief...",
                BodyTemplate: "Dear {{RecipientName}},\n\nI, {{SenderName}} ({{SenderEmail}}), " +
                "submit a DMCA notice for \"{{WorkTitle}}\" ({{RegistrationNumber}}). " +
                "The infringing URL is {{InfringingUrl}}.\n\n{{GoodFaithStatement}}\n\n{{SenderAddress}}"
                );

            var pdf = pdfService.GenerateCopyrightDMCAAsync(dmcaInput).Result;
            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void GeneratePdf_WithUnknownPlaceholders_DoNotThrow()
        {
            var pdfService = new PdfService();

            var ceaseDesistInput = new CeaseDesistInput(
                SenderName: "Δημήτρης",
                SenderAddress: "Athens 105 58",
                RecipientName: "Some Shop",
                RecipientAddress: "Market St.",
                Date: DateTime.UtcNow,
                WorkTitle: "Trademark Ωmega™",
                RegistrationNumber: "TM-555",
                AdditionalFacts: null,
                BodyTemplate: "To {{RecipientName}},\nThis concerns {{WorkTitle}} {{RegistrationNumber}}." +
                "\nUnknown stays: {{Nope}}.\nRegards, {{SenderName}}"
                );

            var pdf = pdfService.GenerateCopyrightCeaseDesistAsync(ceaseDesistInput).Result;
            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void MultipleParagraphs_ProduceLargerPdfThanSingleParagraph()
        {
            var pdfService = new PdfService();

            var baseTemplate = "Para 1 about {{WorkTitle}}." +
                "\n\nPara 2 with {{RegistrationNumber}}.\n\nPara 3 Signed, {{SenderName}}.";

            var oneParagraph = new CeaseDesistInput(
                SenderName: "Alice",
                SenderAddress: "Address1",
                RecipientName: "Tom",
                RecipientAddress: "Address2",
                Date: DateTime.UtcNow,
                WorkTitle: "WorkTitle1",
                RegistrationNumber: "TM-111",
                AdditionalFacts: null,
                BodyTemplate: "Single paragraph with {{WorkTitle}} and {{RegistrationNumber}}. {{SenderName}}"
            );

            var multiParagraphs = new CeaseDesistInput(
                SenderName: "Alice",
                SenderAddress: "Address1",
                RecipientName: "Tom",
                RecipientAddress: "Address2",
                Date: DateTime.UtcNow,
                WorkTitle: "WorkTitle1",
                RegistrationNumber: "TM-111",
                AdditionalFacts: null,
                BodyTemplate: baseTemplate
            );

            var pdf1 = pdfService.GenerateTrademarkCeaseDesistAsync(oneParagraph).Result;
            var pdf2 = pdfService.GenerateTrademarkCeaseDesistAsync(multiParagraphs).Result;

            pdf1.Length.Should().BeGreaterThan(900);
            pdf2.Length.Should().BeGreaterThan(pdf1.Length);
        }

        [Test]
        public void GeneratePdf_WithUnicodeContent_RendersWithoutError()
        {
            var pdfService = new PdfService();

            var input = new CeaseDesistInput(
                SenderName: "Łukasz Černý – ™ ©",
                SenderAddress: "ul. Świętokrzyska 12, Kraków",
                RecipientName: "Empresa Ñandú",
                RecipientAddress: "Córdoba 123",
                Date: DateTime.UtcNow,
                WorkTitle: "Niño & Mañana — Édition spéciale",
                RegistrationNumber: "REG-č-ñ-©-™",
                AdditionalFacts: "Δοκιμή · Тест · اختبار",
                BodyTemplate:
                    "Hello {{RecipientName}},\n\n«{{WorkTitle}}» ({{RegistrationNumber}})\n— Sender: {{SenderName}}\n— Address: {{SenderAddress}}\n\nRegards."
            );

            var pdf = pdfService.GenerateTrademarkCeaseDesistAsync(input).Result;

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }

        [Test]
        public void GeneratePdf_WithNullAdditionalFacts_DoesNotBreakPdf()
        {
            var pdfService = new PdfService();

            var input = new CeaseDesistInput(
               SenderName: "Alice",
                SenderAddress: "Address1",
                RecipientName: "Tom",
                RecipientAddress: "Address2",
                Date: DateTime.UtcNow,
                WorkTitle: "WorkTitle1",
                RegistrationNumber: "TM-111",
                AdditionalFacts: null,
                BodyTemplate: "C&D for {{WorkTitle}} ({{RegistrationNumber}}). " +
                "{{AdditionalFacts}}\nThanks, {{SenderName}}"
            );

            var pdf = pdfService.GenerateTrademarkCeaseDesistAsync(input).Result;

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }
    }
}
