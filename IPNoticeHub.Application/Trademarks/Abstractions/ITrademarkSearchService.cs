using IPNoticeHub.Shared.Infrastructure.Paging;
using IPNoticeHub.Application.Trademarks.DTOs;

namespace IPNoticeHub.Application.Trademarks.Abstractions
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
