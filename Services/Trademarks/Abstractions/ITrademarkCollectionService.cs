using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Abstractions
{
    public interface ITrademarkCollectionService
    {
        Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        Task<bool> IsInCollectionAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken=default);
      
        Task<PagedResult<TrademarkSummaryDTO>> GetUserCollectionAsync(string userId,int currentPage,int resultsPerPage,
            CancellationToken cancellationToken = default);

        Task<PagedResult<TrademarkSummaryDTO>> GetUserCollectionAsync(string userId, CollectionSortBy sortBy,
           int page, int resultsPerPage, CancellationToken ct = default);
    }
}
