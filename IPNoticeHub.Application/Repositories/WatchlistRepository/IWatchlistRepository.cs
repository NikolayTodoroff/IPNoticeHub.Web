using IPNoticeHub.Domain.Entities.Watchlist;

namespace IPNoticeHub.Application.Repositories.WatchlistRepository
{
    public interface IWatchlistRepository
    {
        Task AddOrUndeleteAsync(
            string userId, 
            int trademarkId, 
            int? currentStatusCodeRaw,
            string? 
            currentStatusText, 
            DateTime? 
            currentStatusDateUtc, 
            CancellationToken cancellationToken);

        Task SoftRemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken);

        Task ToggleNotificationsAsync(
            string userId, 
            int trademarkId, 
            bool notificationsEnabled, 
            CancellationToken cancellationToken);

        Task<bool> ExistsAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken);

        Task<IReadOnlyList<Watchlist>> ListByUserAsync(
            string userId, 
            int skip, 
            int take, 
            CancellationToken cancellationToken);

        Task<int> CountByUserAsync(
            string userId, CancellationToken cancellationToken);
    }
}
