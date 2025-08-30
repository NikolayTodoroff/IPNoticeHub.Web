using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Abstractions
{
    public interface ITrademarkSearchService
    {
        Task<PagedResult<TrademarkListItemDTO>> SearchAsync(TrademarkFilterDTO filter,int currentPage, int resultsPerPage, CancellationToken cancellationToken = default);

        Task<TrademarkDetailsDTO?> GetDetailsAsync(Guid publicId);
    }
}
