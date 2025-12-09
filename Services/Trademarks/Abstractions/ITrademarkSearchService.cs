using IPNoticeHub.Common.Infrastructure.Paging;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Abstractions
{
    public interface ITrademarkSearchService
    {
        Task<PagedResult<TrademarkSingleItemDto>>SearchAsync(
            TrademarkFilterDto filter,
            int currentPage, 
            int resultsPerPage, 
            CancellationToken cancellationToken = default);

        Task<TrademarkDetailsDto?>GetDetailsAsync(
            Guid publicId, 
            CancellationToken cancellationToken = default);
    }
}
