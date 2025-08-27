using IPNoticeHub.Services.DTOs.Trademarks;

namespace IPNoticeHub.Services.Abstractions
{
    public interface ITrademarkCollectionService
    {
        Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);
        Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);
        Task<bool> IsInCollectionAsync(string userId, int trademarkId, bool includeSoftDeleted = false);
        Task<PagedResult<TrademarkListItemDTO>> GetUserCollectionAsync(string userId,int page,int pageSize,
            CancellationToken cancellationToken = default);
    }
}
