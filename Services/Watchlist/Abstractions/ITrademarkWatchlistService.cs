using IPNoticeHub.Services.Watchlist.DTOs;

namespace IPNoticeHub.Services.Watchlist.Abstractions
{
    public interface ITrademarkWatchlistService
    {
        Task AddAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken);

        Task RemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken);

        Task ToggleNotificationsAsync(
            string userId, 
            int trademarkId, 
            bool enabled, 
            CancellationToken cancellationToken);

        Task<IReadOnlyList<TrademarkWatchlistItemDto>>GetListByUserAsync(
            string userId, 
            CancellationToken cancellationToken);

        Task<bool>ExistsAsync(
            string userId, 
            int trademarkId, 
            CancellationToken ct);
    }
}
