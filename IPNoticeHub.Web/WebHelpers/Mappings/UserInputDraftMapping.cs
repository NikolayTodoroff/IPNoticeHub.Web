using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using IPNoticeHub.Web.Models.PdfGeneration;
using System.Globalization;
using static IPNoticeHub.Shared.Constants.DateTimeFormats;

namespace IPNoticeHub.Web.WebHelpers.Mappings
{
    public class UserInputDraftMapping
    {
        public static CeaseDesistDraftDto MapCeaseDesistViewModelDraftDto(CeaseDesistViewModel viewModel)
        {
            return new CeaseDesistDraftDto
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

        public static void MapDraftDtoToCeaseDesistViewModel(CeaseDesistViewModel viewModel, CeaseDesistDraftDto dto)
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

                ["WorkTitle"] = mapModel(viewModel.WorkTitle),
                ["RegistrationNumber"] = mapModel(viewModel.RegistrationNumber),

                ["SenderName"] = mapModel(viewModel.SenderName),
                ["SenderAddress"] = mapModel(viewModel.SenderAddress),
                ["SenderEmail"] = mapModel(viewModel.SenderEmail),

                ["RecipientName"] = mapModel(viewModel.RecipientName),
                ["RecipientAddress"] = mapModel(viewModel.RecipientAddress),
                ["RecipientEmail"] = mapModel(viewModel.RecipientEmail),

                ["InfringingUrl"] = mapModel(viewModel.InfringingUrl),
                ["AdditionalFacts"] = mapModel(viewModel.AdditionalFacts)
            };
        }

        public static DmcaDraftDto MapDmcaViewModelDraftDto(DmcaViewModel viewModel)
        {
            return new DmcaDraftDto
            {
                WorkTitle = viewModel.WorkTitle,
                RegistrationNumber = viewModel.RegistrationNumber,

                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                NationOfFirstPublication = viewModel.NationOfFirstPublication,

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

        public static void MapDraftDtoToDmcaViewModel(DmcaViewModel viewModel, DmcaDraftDto dto)
        {
            viewModel.WorkTitle = dto.WorkTitle ?? string.Empty;
            viewModel.RegistrationNumber = dto.RegistrationNumber;

            viewModel.YearOfCreation = dto.YearOfCreation;
            viewModel.DateOfPublication = dto.DateOfPublication;
            viewModel.NationOfFirstPublication = dto.NationOfFirstPublication;

            viewModel.SenderName = dto.SenderName ?? string.Empty;
            viewModel.SenderAddress = dto.SenderAddress ?? string.Empty;
            viewModel.SenderEmail = dto.SenderEmail ?? string.Empty;

            viewModel.RecipientName = dto.RecipientName ?? string.Empty;
            viewModel.RecipientAddress = dto.RecipientAddress ?? string.Empty;
            viewModel.RecipientEmail = dto.RecipientEmail ?? string.Empty;

            viewModel.InfringingUrl = dto.InfringingUrl;
            viewModel.AdditionalFacts = dto.AdditionalFacts;
        }

        public static Dictionary<string, string> MapDmcaViewModelToPlaceholders(DmcaViewModel viewModel)
        {
            static string mapModel(string? v) => v ?? string.Empty;

            return new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow
                    .ToString(DefaultDateTimeFormat.DateTimeFormat,
                    CultureInfo.InvariantCulture),

                ["WorkTitle"] = mapModel(viewModel.WorkTitle),
                ["RegistrationNumber"] = mapModel(viewModel.RegistrationNumber),

                ["YearOfCreation"] =
                viewModel.YearOfCreation?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,

                ["DateOfPublication"] = viewModel.DateOfPublication.HasValue
                    ? viewModel.DateOfPublication.Value.
                    ToString(DefaultDateTimeFormat.DateTimeFormat, CultureInfo.InvariantCulture)
                    : string.Empty,

                ["NationOfFirstPublication"] = mapModel(viewModel.NationOfFirstPublication),

                ["SenderName"] = mapModel(viewModel.SenderName),
                ["SenderAddress"] = mapModel(viewModel.SenderAddress),
                ["SenderEmail"] = mapModel(viewModel.SenderEmail),

                ["RecipientName"] = mapModel(viewModel.RecipientName),
                ["RecipientAddress"] = mapModel(viewModel.RecipientAddress),
                ["RecipientEmail"] = mapModel(viewModel.RecipientEmail),

                ["InfringingUrl"] = mapModel(viewModel.InfringingUrl),
                ["AdditionalFacts"] = mapModel(viewModel.AdditionalFacts),

                ["GoodFaithStatement"] = mapModel(viewModel.GoodFaithStatement)
            };
        }
    }
}
