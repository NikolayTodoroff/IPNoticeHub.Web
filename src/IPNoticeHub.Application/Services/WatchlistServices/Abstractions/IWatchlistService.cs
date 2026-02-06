using IPNoticeHub.Application.DTOs.WatchlistDTOs;

namespace IPNoticeHub.Application.Services.WatchlistService.Abstractions
{
    public interface IWatchlistService
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

        Task<IReadOnlyList<WatchlistItemDto>>GetListByUserAsync(
            string userId, 
            CancellationToken cancellationToken);

        Task<bool>ExistsAsync(
            string userId, 
            int trademarkId, 
            CancellationToken ct);
    }
}
