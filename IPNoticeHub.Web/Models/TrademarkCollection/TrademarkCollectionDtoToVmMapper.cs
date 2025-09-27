using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Web.Models.TrademarkCollection
{
    public static class TrademarkCollectionDtoToVmMapper
    {
        public static TrademarkCollectionIndexViewModel Map(PagedResult<TrademarkSummaryDTO> dtoPage)
        {
            return new()
            {
                CurrentPage = dtoPage.CurrentPage,
                ResultsPerPage = dtoPage.ResultsCountPerPage,
                ResultsCount = dtoPage.ResultsCount,
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
                }).ToList()
            };
        }
    }
}
