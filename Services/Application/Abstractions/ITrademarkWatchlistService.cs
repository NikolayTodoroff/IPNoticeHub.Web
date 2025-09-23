using IPNoticeHub.Services.Application.DTOs;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ITrademarkWatchlistService
    {
        Task AddAsync(string userId, int trademarkId, CancellationToken ct);
        Task RemoveAsync(string userId, int trademarkId, CancellationToken ct);
        Task ToggleNotificationsAsync(string userId, int trademarkId, bool enabled, CancellationToken ct);
        Task<IReadOnlyList<TrademarkWatchlistItemDTO>> GetAsync(string userId, CancellationToken ct);
    }
}
