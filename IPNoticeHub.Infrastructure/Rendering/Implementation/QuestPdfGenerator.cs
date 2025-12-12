using System.Text.RegularExpressions;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IPNoticeHub.Infrastructure.Rendering
{
    public sealed class QuestPdfGenerator : IPdfGenerator
    {
        public byte[] GenerateDocument(PdfLetterDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            QuestPDF.Settings.License = LicenseType.Community;

            var resolvedBody = ApplyTokens(
                dto.BodyTemplate ?? string.Empty, 
                dto.Tokens);

            var document = new LetterDocument(dto, resolvedBody);

            return document.GeneratePdf();
        }

        private static string ApplyTokens(string template, IReadOnlyDictionary<string, string>? tokens)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;
            if (tokens is null || tokens.Count == 0) return template;

            return Regex.Replace(
                template,
                @"\{\{\s*(?<key>[\w\.\-]+)\s*\}\}",
                m =>
                {
                    var key = m.Groups["key"].Value;
                    return tokens.TryGetValue(key, out var value) ? value : m.Value;
                },
                RegexOptions.Compiled);
        }

        private sealed class LetterDocument : IDocument
        {
            private readonly PdfLetterDto dto;
            private readonly string resolvedBody;

            public LetterDocument(PdfLetterDto dto, string resolvedBody)
            {
                this.dto = dto;
                this.resolvedBody = resolvedBody;
            }

            public DocumentMetadata GetMetadata()
                => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }

            private void ComposeHeader(IContainer container)
            {
                container.Column(col =>
                {
                    col.Spacing(6);

                    col.Item().Text(dto.DocumentTitle ?? string.Empty).FontSize(16).SemiBold();

                    col.Item().Text($"{dto.DocumentType} • {dto.DateUtc:yyyy-MM-dd}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken2);

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });
            }

            private void ComposeContent(IContainer container)
            {
                container.Column(col =>
                {
                    col.Spacing(12);

                    // “From / To” block (optional – remove if you don’t want it)
                    col.Item().Element(ComposeParties);

                    // Work / registration info
                    col.Item().Element(ComposeWorkInfo);

                    // Body
                    col.Item().Text(resolvedBody).LineHeight(1.35f);
                });
            }

            private void ComposeParties(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Sender").SemiBold();
                        c.Item().Text(dto.SenderName ?? string.Empty);
                        c.Item().Text(dto.SenderEmail ?? string.Empty);
                        c.Item().Text(dto.SenderAddress ?? string.Empty);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Recipient").SemiBold();
                        c.Item().Text(dto.RecipientName ?? string.Empty);
                        c.Item().Text(dto.RecipientEmail ?? string.Empty);
                        c.Item().Text(dto.RecipientAddress ?? string.Empty);
                    });
                });
            }

            private void ComposeWorkInfo(IContainer container)
            {
                container.Column(c =>
                {
                    c.Spacing(4);
                    if (!string.IsNullOrWhiteSpace(dto.WorkTitle))
                        c.Item().Text($"Work: {dto.WorkTitle}");

                    if (!string.IsNullOrWhiteSpace(dto.RegistrationNumber))
                        c.Item().Text($"Registration #: {dto.RegistrationNumber}");

                    if (!string.IsNullOrWhiteSpace(dto.InfringingUrl))
                        c.Item().Text($"Infringing URL: {dto.InfringingUrl}");
                });
            }
        }
    }
}
