using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.TrademarkCollection;
using IPNoticeHub.Web.Models.Trademarks;

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
    }
}
