using IPNoticeHub.Services.Application.DTOs;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ITrademarkWatchlistService
    {
        Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken);
        Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken);
        Task ToggleNotificationsAsync(string userId, int trademarkId, bool enabled, CancellationToken cancellationToken);
        Task<IReadOnlyList<TrademarkWatchlistItemDTO>> GetListByUserAsync(string userId, CancellationToken cancellationToken);
    }
}
