using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.Rendering.Abstractions;
using QuestPDF.Fluent;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;

namespace IPNoticeHub.Infrastructure.Rendering;

public sealed class QuestPdfGenerator : IPdfGenerator
{
    private readonly ITemplateTokenReplacer tokenReplacer;

    public QuestPdfGenerator(ITemplateTokenReplacer tokenReplacer)
    {
        this.tokenReplacer = tokenReplacer;
    }

    public byte[] GenerateDocument(PdfLetterDto dto)
    {
        string letterTemplate = tokenReplacer.ReplaceTemplate(dto.BodyTemplate, dto.Tokens);
        letterTemplate = letterTemplate.Replace("\r\n", "\n");

        var bytes = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text(dto.SenderName).SemiBold();

                    if (!string.IsNullOrWhiteSpace(dto.SenderAddress))
                        col.Item().Text(dto.SenderAddress);

                    col.Item().Text(dto.DateUtc.ToString(DateTimeFormat)).FontSize(10);
                });

                page.Content().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(dto.RecipientName) ||
                        !string.IsNullOrWhiteSpace(dto.RecipientAddress))
                    {
                        col.Item()
                            .PaddingBottom(8)
                            .Text($"{dto.RecipientName}\n{dto.RecipientAddress}".Trim());
                    }

                    col.Item().Text(letterTemplate).AlignLeft().LineHeight(1.4f);
                });

                page.Footer().AlignRight().Text("Generated with IPNoticeHub").
                FontSize(9);
            });
        }).GeneratePdf();

        return bytes;
    }
}
