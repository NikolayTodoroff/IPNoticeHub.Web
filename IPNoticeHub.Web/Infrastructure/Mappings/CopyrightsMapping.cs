using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants;

namespace IPNoticeHub.Web.Infrastructure.Mappings
{
    public class CopyrightsMapping
    {
        public static CopyrightCreateDto MapNewCopyrightViewModelToDto(CopyrightCreateViewModel viewModel)
        {
            return new CopyrightCreateDto()
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                WorkType = viewModel.WorkType,
                OtherWorkType = viewModel.OtherWorkType,
                Title = viewModel.Title,
                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                Owner = viewModel.Owner,
                NationOfFirstPublication = viewModel.NationOfFirstPublication
            };
        }

        public static CopyrightEditViewModel MapDetailsDtoToEditViewModel(CopyrightDetailsDTO dto,
            CopyrightWorkType workType, string? otherText)
        {
            return new CopyrightEditViewModel()
            {
                PublicId = dto.PublicId,
                RegistrationNumber = dto.RegistrationNumber,
                WorkType = workType,
                OtherWorkType = otherText,
                Title = dto.Title,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                Owner = dto.Owner,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };
        }

        public static CopyrightEditDTO MapEditViewModelToDto(CopyrightEditViewModel viewModel)
        {
            return new CopyrightEditDTO()
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                WorkType = viewModel.WorkType,
                OtherWorkType = viewModel.OtherWorkType,
                Title = viewModel.Title,
                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                Owner = viewModel.Owner,
                NationOfFirstPublication = viewModel.NationOfFirstPublication
            };
        }

        public static CopyrightDetailsViewModel MapDetailsDtoToViewModel(CopyrightDetailsDTO dto)
        {
            return new CopyrightDetailsViewModel()
            {
                PublicId = dto.PublicId,
                RegistrationNumber = dto.RegistrationNumber,
                TypeOfWork = dto.TypeOfWork,
                Title = dto.Title,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                Owner = dto.Owner,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };
        }

        public static DMCAViewModel MapDetailsDtoToDmcaViewModel(CopyrightDetailsDTO dto, Guid publicId)
        {
            return new DMCAViewModel()
            {
                PublicId = publicId,
                WorkTitle = dto.Title,
                RegistrationNumber = dto.RegistrationNumber,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };
        }

        public static DMCAInput MapDmcaViewModelToInput(DMCAViewModel viewModel, DateTime? now = null)
        {
            return new DMCAInput(
                SenderName: viewModel.SenderName,
                SenderEmail: viewModel.SenderEmail,
                SenderAddress: viewModel.SenderAddress,
                RecipientName: viewModel.RecipientName,
                RecipientEmail: viewModel.RecipientEmail ?? string.Empty,
                RecipientAddress: viewModel.RecipientAddress ?? string.Empty,
                Date: now ?? DateTime.UtcNow,
                WorkTitle: viewModel.WorkTitle,
                RegistrationNumber: viewModel.RegistrationNumber ?? string.Empty,
                YearOfCreation: viewModel.YearOfCreation,
                DateOfPublication: viewModel.DateOfPublication,
                NationOfFirstPublication: viewModel.NationOfFirstPublication,
                InfringingUrl: viewModel.InfringingUrl,
                GoodFaithStatement: viewModel.GoodFaithStatement,
                BodyTemplate: viewModel.BodyTemplate);
        }

        public static Dictionary<string, string> MapDmcaViewModelToPlaceholders(DMCAViewModel viewModel)
        {
            static string mapModel(string? v) => v ?? string.Empty;

            return new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow
                    .ToString(FormattingConstants.DateTimeFormat, CultureInfo.InvariantCulture),

                ["RecipientName"] = mapModel(viewModel.RecipientName),
                ["RecipientAddress"] = mapModel(viewModel.RecipientAddress),
                ["RecipientEmail"] = mapModel(viewModel.RecipientEmail),

                ["SenderName"] = mapModel(viewModel.SenderName),
                ["SenderAddress"] = mapModel(viewModel.SenderAddress),
                ["SenderEmail"] = mapModel(viewModel.SenderEmail),

                ["WorkTitle"] = mapModel(viewModel.WorkTitle),
                ["RegistrationNumber"] = mapModel(viewModel.RegistrationNumber),
                ["InfringingUrl"] = mapModel(viewModel.InfringingUrl),

                ["YearOfCreation"] = mapModel(viewModel.YearOfCreation?.ToString()),
                ["DateOfPublication"] = mapModel(viewModel.DateOfPublication?
                    .ToString(FormattingConstants.DateTimeFormat, CultureInfo.InvariantCulture)),

                ["NationOfFirstPublication"] = mapModel(viewModel.NationOfFirstPublication),
                ["GoodFaithStatement"] = mapModel(viewModel.GoodFaithStatement)
            };
        }

        public static Dictionary<string, string> MapCeaseDesistViewModelToPlaceholders(CeaseDesistViewModel viewModel)
        {
            static string mapModel(string? v) => v ?? string.Empty;

            return new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow
                    .ToString(FormattingConstants.DateTimeFormat, CultureInfo.InvariantCulture),

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

        public static CeaseDesistViewModel MapDetailsDtoToCeaseDesistViewModel(CopyrightDetailsDTO dto,Guid publicId)
        {
            return new CeaseDesistViewModel()
            {
                PublicId = publicId,
                WorkTitle = dto.Title,
                RegistrationNumber = dto.RegistrationNumber
            };
        }

        public static CeaseDesistInput MapCeaseDesistViewModelToInput(CeaseDesistViewModel viewModel)
        {
            return new CeaseDesistInput(

                SenderName: viewModel.SenderName,
                SenderAddress: viewModel.SenderAddress,
                RecipientName: viewModel.RecipientName,
                RecipientAddress: viewModel.RecipientAddress,
                Date: DateTime.UtcNow,
                WorkTitle: viewModel.WorkTitle,
                RegistrationNumber: viewModel.RegistrationNumber ?? string.Empty,
                AdditionalFacts: viewModel.AdditionalFacts,
                BodyTemplate: viewModel.BodyTemplate);
        }
    }
}
