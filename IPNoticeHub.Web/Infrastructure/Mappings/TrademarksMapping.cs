using Humanizer;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.Models.TrademarkCollection;
using IPNoticeHub.Web.Models.Trademarks;
using IPNoticeHub.Web.ViewModels.Trademarks;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants;

namespace IPNoticeHub.Web.Infrastructure.Mappings
{
    public class TrademarksMapping
    {
        public static TrademarkCollectionViewModel MapCollectionDtoToViewModel(PagedResult<TrademarkSummaryDTO> dtoPage)
        {
            return new()
            {
                Total = dtoPage.ResultsCount,
                CurrentPage = dtoPage.CurrentPage,
                PageSize = dtoPage.ResultsCountPerPage,
                Results = dtoPage.Results.Select(s => new TrademarkCollectionSingleItemViewModel
                {
                    Id = s.Id,
                    PublicId = s.PublicId,
                    Wordmark = s.Wordmark,
                    SourceId = s.SourceId,
                    Owner = s.Owner,
                    Status = s.Status,
                    Classes = s.Classes ?? Array.Empty<int>(),
                    Provider = s.Provider
                }).
                ToList()
            };
        }

        public static TrademarkCollectionIndexViewModel MapCollectionIndexDtoToViewModel(TrademarkFilterViewModel filter, PagedResult<TrademarkSummaryDTO> resultsPageDTO)
        {
            return new TrademarkCollectionIndexViewModel
            {
                SearchTerm = filter.SearchTerm?.Trim(),
                SearchBy = filter.SearchBy,
                Provider = filter.Provider,
                Status = filter.Status,
                ClassNumbers = filter.ClassNumbers ?? Array.Empty<int>(),
                ExactMatch = filter.ExactMatch,

                CurrentPage = resultsPageDTO.CurrentPage,
                ResultsPerPage = resultsPageDTO.ResultsCountPerPage,
                ResultsCount = resultsPageDTO.ResultsCount,

                Results = resultsPageDTO.Results.Select(MapTrademarkSummaryViewModel).ToList()
            };
        }

        private static TrademarkSummaryViewModel MapTrademarkSummaryViewModel(TrademarkSummaryDTO summaryDTO)
        {
            return new TrademarkSummaryViewModel()
            {
                Id = summaryDTO.Id,
                PublicId = summaryDTO.PublicId,
                Wordmark = summaryDTO.Wordmark,
                SourceId = summaryDTO.SourceId,
                Owner = summaryDTO.Owner,
                Status = summaryDTO.Status,
                Classes = summaryDTO.Classes ?? Array.Empty<int>(),
                Provider = summaryDTO.Provider
            };
        }

        public static TrademarkDetailsViewModel MapDetailsDtoToViewModel(TrademarkDetailsDTO dto,
            bool isInCollection, bool isInWatchlist, bool isAuthenticated)
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

        public static CeaseDesistViewModel MapCeaseDesistViewModel(Guid publicId,string wordMark,string registrationNumber)
        {
            return new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = wordMark ?? registrationNumber ?? "Trademark",
                RegistrationNumber = registrationNumber
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
                BodyTemplate: viewModel.BodyTemplate
            );
        }

        public static Dictionary<string, string> MapCeaseDesistViewModellToPlaceholders(CeaseDesistViewModel viewModel)
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
    }
}
