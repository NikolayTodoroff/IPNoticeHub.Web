using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Infrastructure.Paging;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Services.DocumentLibrary.DTOs;
using IPNoticeHub.Services.PdfGeneration.Abstractions;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using System.Globalization;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;

namespace IPNoticeHub.Web.Infrastructure.Mappings
{
    public class CopyrightsMapping
    {
        public static CopyrightCollectionViewModel MapCollectionDtoToViewModel(PagedResult<CopyrightSingleItemDto> dto)
        {
            return new CopyrightCollectionViewModel()
            {
                Total = dto.ResultsCount,
                CurrentPage = dto.CurrentPage,
                PageSize = dto.ResultsCountPerPage,
                Results = dto.Results.
                Select(csi => new CopyrightSingleItemViewModel
                {
                    Id = csi.Id,
                    PublicId = csi.PublicId,
                    Owner = csi.Owner,
                    RegistrationNumber = csi.RegistrationNumber,
                    TypeOfWork = csi.TypeOfWork,
                    Title = csi.Title,
                    YearOfCreation = csi.YearOfCreation,
                    DateOfPublication = csi.DateOfPublication,
                    DateAdded = csi.DateAdded
                }).
                ToList()
            };
        }

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

        public static CopyrightEditViewModel MapDetailsDtoToEditViewModel(CopyrightDetailsDto dto,
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

        public static CopyrightEditDto MapEditViewModelToDto(CopyrightEditViewModel viewModel)
        {
            return new CopyrightEditDto()
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

        public static CopyrightDetailsViewModel MapDetailsDtoToViewModel(CopyrightDetailsDto dto)
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

        public static DMCAViewModel MapDetailsDtoToDmcaViewModel(CopyrightDetailsDto dto, Guid publicId)
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
                InfringingUrl: viewModel.InfringingUrl!,
                GoodFaithStatement: viewModel.GoodFaithStatement,
                BodyTemplate: viewModel.BodyTemplate);
        }

        public static Dictionary<string, string> MapDmcaViewModelToPlaceholders(DMCAViewModel viewModel)
        {
            static string mapModel(string? v) => v ?? string.Empty;

            return new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow
                    .ToString(DateTimeFormat, CultureInfo.InvariantCulture),

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
                    .ToString(DateTimeFormat, CultureInfo.InvariantCulture)),

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
                    .ToString(DateTimeFormat, CultureInfo.InvariantCulture),

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

        public static CeaseDesistViewModel MapDetailsDtoToCeaseDesistViewModel(CopyrightDetailsDto dto,Guid publicId)
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

        public static DocumentCreateDto MapCdViewModelToDocCreateDto(
            CeaseDesistViewModel viewModel)
        {
            return new DocumentCreateDto
            {
                RelatedPublicId = viewModel.PublicId,
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = null,
                IpTitle = viewModel.WorkTitle ?? "Intellectual property identified by registration",
                RegistrationNumber = viewModel.RegistrationNumber,

                SenderName = viewModel.SenderName,
                SenderAddress = viewModel.SenderAddress,
                SenderEmail = viewModel.SenderEmail,

                RecipientName = viewModel.RecipientName,
                RecipientAddress = viewModel.RecipientAddress,
                RecipientEmail = viewModel.RecipientEmail,

                LetterDate = DateTime.UtcNow,
                BodyTemplate = viewModel.BodyTemplate
            };
        }

        public static DocumentCreateDto MapDmcaViewModelToDocCreateDto(
            DMCAViewModel viewModel)
        {
            return new DocumentCreateDto
            {
                RelatedPublicId = viewModel.PublicId,
                SourceType = DocumentSourceType.Copyright,
                TemplateType = LetterTemplateType.Dmca,
                DocumentTitle = null,
                IpTitle = viewModel.WorkTitle ?? "Intellectual property identified by registration",
                RegistrationNumber = viewModel.RegistrationNumber,

                SenderName = viewModel.SenderName,
                SenderAddress = viewModel.SenderAddress,
                SenderEmail = viewModel.SenderEmail,

                RecipientName = viewModel.RecipientName,
                RecipientAddress = viewModel.RecipientAddress,
                RecipientEmail = viewModel.RecipientEmail,

                InfringingUrl = viewModel.InfringingUrl,
                GoodFaithStatement = viewModel.GoodFaithStatement,
                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                NationOfFirstPublication = viewModel.NationOfFirstPublication,

                LetterDate = DateTime.UtcNow,
                BodyTemplate = viewModel.BodyTemplate
            };
        }
    }
}
