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
            Task.FromResult(BuildLetter(data.BodyTemplate, BuildDMCATemplateVars(data)));
        }

        public Task<byte[]> GenerateTrademarkCeaseDesistAsync(CeaseDesistInput data, CancellationToken cancellation = default)
        {
            Task.FromResult(BuildLetter(data.BodyTemplate, BuildCnDTemplateVars(data)));
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
    }
}
