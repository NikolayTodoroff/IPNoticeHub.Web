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
        public Task<byte[]> GenerateCopyrightCeaseDesistAsync(CeaseDesistInput data, CancellationToken cancellation = default)
        {
            return Task.FromResult(BuildLetter(data.BodyTemplate, BuildCnDTemplateVars(data)));
        }

        public Task<byte[]> GenerateCopyrightDMCAAsync(DMCAInput data, CancellationToken cancellation = default)
        {
            return Task.FromResult(BuildLetter(data.BodyTemplate, BuildDMCATemplateVars(data)));
        }

        public Task<byte[]> GenerateTrademarkCeaseDesistAsync(CeaseDesistInput data, CancellationToken cancellation = default)
        {
            return Task.FromResult(BuildLetter(data.BodyTemplate, BuildCnDTemplateVars(data)));
        }

        private static Dictionary<string, string> BuildCnDTemplateVars(CeaseDesistInput input) => new()
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

        private static Dictionary<string, string> BuildDMCATemplateVars(DMCAInput input) => new()
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

        private static byte[] BuildLetter(string template,Dictionary<string, string> vars)
        {

        }

        private static IEnumerable<string> SplitIntoParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<string>();
            }       

            // Split whenever there are two or more consecutive blank lines
            var parts = Regex.Split(text.Trim(), @"(\r?\n){2,}");

            // Return only non-empty paragraphs
            return parts.Select(p => p.Trim()).Where(p => p.Length > 0);
        }

        private static string ReplaceTemplate(string template, Dictionary<string, string> vars)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }           

            return Regex.Replace(template, @"{{\s*(\w+)\s*}}", match =>
            {
                string key = match.Groups[1].Value;

                // If we have a value for this key, use it; otherwise leave the placeholder unchanged
                if (vars.TryGetValue(key, out string? value))
                {
                    return value ?? string.Empty;
                }             

                return match.Value; // keeps {{UnknownKey}} visible
            });
        }
    }
}
