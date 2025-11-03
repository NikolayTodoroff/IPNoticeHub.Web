using FluentAssertions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Implementations;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.PdfTests
{
    public class PdfServiceTests
    {
        [Test]
        public void GeneratesNonEmptyPdf_ForSimpleTemplate()
        {
            var pdfService = new PdfService();

            var input = new CeaseDesistInput(
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

            var pdf = pdfService.GenerateTrademarkCeaseDesistAsync(input).Result;

            pdf.Should().NotBeNull();
            pdf.Length.Should().BeGreaterThan(1000);
        }
    }
}
