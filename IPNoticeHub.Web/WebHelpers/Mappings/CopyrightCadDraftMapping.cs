using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using IPNoticeHub.Web.Models.PdfGeneration;
using System.Globalization;
using System.Runtime.Serialization;
using static IPNoticeHub.Shared.Constants.DateTimeFormats;

namespace IPNoticeHub.Web.WebHelpers.Mappings
{
    public class CopyrightCadDraftMapping
    {
        public static CopyrightCadDraftDto MapCeaseDesistViewModelDraftDto(CeaseDesistViewModel viewModel)
        {
            return new CopyrightCadDraftDto
            {
                WorkTitle = viewModel.WorkTitle,
                RegistrationNumber = viewModel.RegistrationNumber,

                SenderName = viewModel.SenderName,
                SenderAddress = viewModel.SenderAddress,
                SenderEmail = viewModel.SenderEmail,

                RecipientName = viewModel.RecipientName,
                RecipientAddress = viewModel.RecipientAddress,
                RecipientEmail = viewModel.RecipientEmail,

                InfringingUrl = viewModel.InfringingUrl,
                AdditionalFacts = viewModel.AdditionalFacts
            };
        }

        public static void MapDraftDtoToCeaseDesistViewModel(CeaseDesistViewModel viewModel, CopyrightCadDraftDto dto)
        {
            viewModel.WorkTitle = dto.WorkTitle ?? string.Empty;
            viewModel.RegistrationNumber = dto.RegistrationNumber;

            viewModel.SenderName = dto.SenderName ?? string.Empty;
            viewModel.SenderAddress = dto.SenderAddress ?? string.Empty;
            viewModel.SenderEmail = dto.SenderEmail ?? string.Empty;

            viewModel.RecipientName = dto.RecipientName ?? string.Empty;
            viewModel.RecipientAddress = dto.RecipientAddress ?? string.Empty;
            viewModel.RecipientEmail = dto.RecipientEmail ?? string.Empty;

            viewModel.InfringingUrl = dto.InfringingUrl;
            viewModel.AdditionalFacts = dto.AdditionalFacts;
        }

        public static Dictionary<string, string> MapCeaseDesistViewModelToPlaceholders(CeaseDesistViewModel viewModel)
        {
            static string mapModel(string? v) => v ?? string.Empty;

            return new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow
                    .ToString(DefaultDateTimeFormat.DateTimeFormat, 
                    CultureInfo.InvariantCulture),

                ["RecipientName"] = mapModel(viewModel.RecipientName),
                ["RecipientAddress"] = mapModel(viewModel.RecipientAddress),
                ["RecipientEmail"] = mapModel(viewModel.RecipientEmail),

                ["SenderName"] = mapModel(viewModel.SenderName),
                ["SenderAddress"] = mapModel(viewModel.SenderAddress),
                ["SenderEmail"] = mapModel(viewModel.SenderEmail),

                ["InfringingUrl"] = mapModel(viewModel.InfringingUrl),
                ["WorkTitle"] = mapModel(viewModel.WorkTitle),
                ["RegistrationNumber"] = mapModel(viewModel.RegistrationNumber),
                ["AdditionalFacts"] = mapModel(viewModel.AdditionalFacts)
            };
        }
    }
}
