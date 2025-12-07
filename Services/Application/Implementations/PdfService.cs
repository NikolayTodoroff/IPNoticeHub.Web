using IPNoticeHub.Services.Application.Abstractions;
using static IPNoticeHub.Common.ValidationConstants;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace IPNoticeHub.Services.Application.Implementations
{
    public sealed class PdfService : IPdfService
    {
        public Task<byte[]> GenerateCopyrightCeaseDesistAsync(
            CeaseDesistInput data, 
            CancellationToken cancellation = default)
        {
            return Task.FromResult(
                BuildLetter(data.BodyTemplate, BuildCnDTemplateVars(data)));
        }

        public Task<byte[]> GenerateCopyrightDMCAAsync(
            DMCAInput data, 
            CancellationToken cancellation = default)
        {
            return Task.FromResult(
                BuildLetter(data.BodyTemplate, BuildDMCATemplateVars(data)));
        }

        public Task<byte[]> GenerateTrademarkCeaseDesistAsync(
            CeaseDesistInput data, 
            CancellationToken cancellation = default)
        {
            return Task.FromResult(
                BuildLetter(data.BodyTemplate, BuildCnDTemplateVars(data)));
        }

        public Task<byte[]> CaptureDocumentSnapshotAsync(
            string title,
            string body,
            DateTime createdOn,
            CancellationToken cancellation = default)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));

                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(title)
                                .SemiBold()
                                .FontSize(14);

                            row.ConstantItem(120)
                               .AlignRight()
                               .Text(createdOn.ToString("yyyy-MM-dd"));
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(0.5f);

                        col.Item().PaddingTop(20)
                           .Text(body)
                           .LineHeight(1.4f);
                    });
                });
            });

            var bytes = document.GeneratePdf();
            return Task.FromResult(bytes);
        }

        private static Dictionary<string, string> BuildCnDTemplateVars(
            CeaseDesistInput input) => new()
        {
            ["SenderName"] = input.SenderName,
            ["SenderAddress"] = input.SenderAddress,
            ["RecipientName"] = input.RecipientName,
            ["RecipientAddress"] = input.RecipientAddress,
            ["Date"] = input.Date.ToString(FormattingConstants.DateTimeFormat),
            ["WorkTitle"] = input.WorkTitle,
            ["RegistrationNumber"] = input.RegistrationNumber,
            ["AdditionalFacts"] = input.AdditionalFacts ?? string.Empty
        };

        private static Dictionary<string, string> BuildDMCATemplateVars(
            DMCAInput input) => new()
        {
            ["SenderName"] = input.SenderName,
            ["SenderEmail"] = input.SenderEmail,
            ["SenderAddress"] = input.SenderAddress,
            ["RecipientName"] = input.RecipientName,
            ["RecipientEmail"] = input.RecipientEmail ?? string.Empty,
            ["RecipientAddress"] = input.RecipientAddress ?? string.Empty,
            ["Date"] = input.Date.ToString(FormattingConstants.DateTimeFormat),
            ["WorkTitle"] = input.WorkTitle,
            ["RegistrationNumber"] = input.RegistrationNumber,
            ["YearOfCreation"] = input.YearOfCreation?.ToString() ?? string.Empty,
            ["DateOfPublication"] = input.DateOfPublication?.ToString(FormattingConstants.DateTimeFormat) ?? string.Empty,
            ["NationOfFirstPublication"] = input.NationOfFirstPublication ?? string.Empty,
            ["InfringingUrl"] = input.InfringingUrl,
            ["GoodFaithStatement"] = input.GoodFaithStatement
        };

        private static byte[] BuildLetter(
            string template,
            Dictionary<string, string> vars)
        {
            string resolved = ReplaceTemplate(template, vars);

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(vars.GetValueOrDefault("SenderName")).SemiBold();

                        var address = vars.GetValueOrDefault("SenderAddress");

                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            col.Item().Text(address);
                        }

                        var date = vars.GetValueOrDefault("Date");

                        if (!string.IsNullOrWhiteSpace(date))
                        {
                            col.Item().Text(date).FontSize(10).FontColor(Colors.Grey.Darken2);
                        }
                    });

                    page.Content().Column(col =>
                    {
                        var recipientName = vars.GetValueOrDefault("RecipientName");
                        var recipientAddress = vars.GetValueOrDefault("RecipientAddress");

                        if (!string.IsNullOrWhiteSpace(recipientName) || !string.IsNullOrWhiteSpace(recipientAddress))
                        {
                            col.Item().PaddingBottom(8).Text($"{recipientName}\n{recipientAddress}".Trim());
                        }

                        foreach (var paragraph in SplitIntoParagraphs(resolved))
                        {
                            col.Item().PaddingBottom(6).Text(paragraph).AlignLeft();
                        }
                    });

                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Generated with IPNoticeHub").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return bytes;
        }

        private static IEnumerable<string> SplitIntoParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<string>();

            var parts = Regex.Split(text.Trim(), @"(\r?\n){2,}");

            return parts.Select(p => p.Trim()).Where(p => p.Length > 0);
        }

        private static string ReplaceTemplate(string template, Dictionary<string, string> vars)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            return Regex.Replace(template, @"{{\s*(\w+)\s*}}", match =>
            {
                string key = match.Groups[1].Value;

                if (vars.TryGetValue(key, out string? value)) return value ?? string.Empty;

                return match.Value; 
            });
        }
    }
}
