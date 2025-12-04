using Humanizer;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.Models.TrademarkCollection;
using IPNoticeHub.Web.Models.Trademarks;
using IPNoticeHub.Web.ViewModels.Trademarks;

namespace IPNoticeHub.Web.Infrastructure.Mapping
{
    public class TrademarksMapping
    {
        public static TrademarkCollectionIndexViewModel TrademarkDtoToViewModelMapping(PagedResult<TrademarkSummaryDTO> dtoPage)
        {
            return new()
            {
                Total = dtoPage.ResultsCount,
                CurrentPage = dtoPage.CurrentPage,
                PageSize = dtoPage.ResultsCountPerPage,
                Results = dtoPage.Results.Select(s => new TrademarkCollectionItemViewModel
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

        public static TrademarksIndexViewModel TrademarkIndexViewModelMapping(TrademarkFilterViewModel filter, PagedResult<TrademarkSummaryDTO> resultsPageDTO)
        {
            return new TrademarksIndexViewModel
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

                Results = resultsPageDTO.Results.Select(TrademarkSummaryMapping).ToList()
            };
        }

        private static TrademarkSummaryViewModel TrademarkSummaryMapping(TrademarkSummaryDTO summaryDTO)
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

        public static TrademarkDetailsViewModel TrademarksDetailsViewModelMapping(TrademarkDetailsDTO dto,
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

        public static CeaseDesistViewModel CeaseDesistViewModelMapping(Guid publicId,string wordMark,string registrationNumber)
        {
            return new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = wordMark ?? registrationNumber ?? "Trademark",
                RegistrationNumber = registrationNumber
            };
        }
    }
}
