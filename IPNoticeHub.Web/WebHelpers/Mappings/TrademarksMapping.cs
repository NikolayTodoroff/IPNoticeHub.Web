using Humanizer;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.Models.Trademarks;
using IPNoticeHub.Web.ViewModels.Trademarks;
using System.Globalization;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Web.WebHelpers.Mappings
{
    public class TrademarksMapping
    {
        public static TrademarkCollectionViewModel MapCollectionDtoToViewModel(
            PagedResult<TrademarkSingleItemDto> dto)
        {
            return new()
            {
                Total = dto.ResultsCount,
                CurrentPage = dto.CurrentPage,
                PageSize = dto.ResultsCountPerPage,
                Results = dto.Results.
                Select(tsi => new TrademarkSingleItemViewModel
                {
                    Id = tsi.Id,
                    PublicId = tsi.PublicId,
                    Wordmark = tsi.Wordmark,
                    SourceId = tsi.SourceId,
                    Owner = tsi.Owner,
                    Status = tsi.Status,
                    Classes = tsi.Classes ?? Array.Empty<int>(),
                    Provider = tsi.Provider
                }).
                ToList()
            };
        }

        public static TrademarkDetailsViewModel MapDetailsDtoToViewModel(
            TrademarkDetailsDto dto,
            bool isInCollection, 
            bool isInWatchlist, 
            bool isAuthenticated)
        {
            return new TrademarkDetailsViewModel
            {
                Id = dto.Id,
                PublicId = dto.PublicId,
                Wordmark = dto.Wordmark,
                Owner = dto.Owner,
                SourceId = dto.SourceId,
                RegistrationNumber = dto.RegistrationNumber,
                Status = dto.Status,
                GoodsAndServices = dto.GoodsAndServices,
                FilingDate = dto.FilingDate,
                RegistrationDate = dto.RegistrationDate,
                MarkImageUrl = dto.MarkImageUrl,
                Provider = dto.Provider,
                Classes = dto.Classes,
                IsInCollection = isInCollection,
                IsInWatchlist = isInWatchlist,
                IsAuthenticated = isAuthenticated
            };
        }

        public static CeaseDesistViewModel MapCeaseDesistViewModel(
            Guid publicId,
            string wordMark,
            string registrationNumber)
        {
            return new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = wordMark ?? registrationNumber ?? "Trademark",
                RegistrationNumber = registrationNumber
            };
        }

        public static LetterInputDto MapCeaseDesistViewModelInput(CeaseDesistViewModel viewModel)
        {
            return new LetterInputDto
            {
                WorkTitle = viewModel.WorkTitle,
                RegistrationNumber = viewModel.RegistrationNumber ?? string.Empty,
                LetterDateUtc = DateTime.UtcNow,

                SenderName = viewModel.SenderName,
                SenderAddress = viewModel.SenderAddress,
                SenderEmail = viewModel.SenderEmail,

                RecipientName = viewModel.RecipientName,
                RecipientAddress = viewModel.RecipientAddress,
                RecipientEmail = viewModel.RecipientEmail,

                InfringingUrl = viewModel.InfringingUrl,
                AdditionalFacts = viewModel.AdditionalFacts,
                BodyTemplate = viewModel.BodyTemplate
            };
        }

        public static Dictionary<string, string> MapCeaseDesistViewModellToPlaceholders(
            CeaseDesistViewModel viewModel)
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

        public static DocumentCreateDto MapCdViewModelToDocCreateDto(
            CeaseDesistViewModel viewModel)
        {
            return new DocumentCreateDto
            {
                RelatedPublicId = viewModel.PublicId,
                SourceType = DocumentSourceType.Trademark,
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
    }
}
