using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Web.Models.Trademarks
{
    /// <summary>
    /// Maps service DTOs (PagedResult<TrademarkSummaryDTO>) + TrademarkFilterViewModel
    /// into the TrademarksIndexViewModel consumed by the Razor view.
    /// </summary>
    public static class TrademarksIndexDtoToVmMapper
    {
        public static TrademarksIndexViewModel MapToIndexViewModel(TrademarkFilterViewModel filter,PagedResult<TrademarkSummaryDTO> resultsPageDTO)
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

                Results = resultsPageDTO.Results.Select(MapResultsToTrademarkSummary).ToList()
            };
        }

        private static TrademarkSummaryViewModel MapResultsToTrademarkSummary(TrademarkSummaryDTO summaryDTO)
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

