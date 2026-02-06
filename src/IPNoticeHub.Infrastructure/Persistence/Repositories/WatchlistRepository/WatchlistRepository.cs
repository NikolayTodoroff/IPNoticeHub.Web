using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Domain.Entities.Watchlist;

namespace IPNoticeHub.Infrastructure.Persistence.Repositories.WatchlistRepository
{
    public class WatchlistRepository : IWatchlistRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public WatchlistRepository(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddOrUndeleteAsync(
            string userId, 
            int trademarkId, 
            int? currentStatusCodeRaw,
            string? currentStatusText, 
            DateTime? currentStatusDateUtc, 
            CancellationToken cancellationToken)
        {
            var watchlist = await dbContext.
                Watchlists.IgnoreQueryFilters().
                FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId, 
                cancellationToken);

            if (watchlist is null)
            {
                watchlist = new Watchlist
                {
                    UserId = userId,
                    TrademarkId = trademarkId,
                    IsDeleted = false,
                    NotificationsEnabled = false,
                    AddedOnUtc = DateTime.UtcNow,
                    InitialStatusCodeRaw = currentStatusCodeRaw,
                    InitialStatusText = currentStatusText,
                    InitialStatusDateUtc = currentStatusDateUtc
                };

                await dbContext.Watchlists.AddAsync(
                    watchlist, 
                    cancellationToken);
            }

            else
            {
                if (watchlist.IsDeleted) watchlist.IsDeleted = false;

                if (watchlist.AddedOnUtc == default)
                    watchlist.AddedOnUtc = DateTime.UtcNow;

                if (watchlist.InitialStatusCodeRaw is null)
                    watchlist.InitialStatusCodeRaw = currentStatusCodeRaw;

                if (string.IsNullOrEmpty(watchlist.InitialStatusText))
                    watchlist.InitialStatusText = currentStatusText;

                if (watchlist.InitialStatusDateUtc is null)
                    watchlist.InitialStatusDateUtc = currentStatusDateUtc;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CountByUserAsync(
            string userId, 
            CancellationToken cancellationToken)
        {
            return await dbContext.Watchlists.
                  CountAsync(
                ut => ut.UserId == userId && 
                !ut.IsDeleted, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken)
        {
            return await dbContext.Watchlists.
                 AnyAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId && 
                !ut.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<Watchlist>> ListByUserAsync(
            string userId, 
            int skip, 
            int take, 
            CancellationToken cancellationToken)
        {
            return await dbContext.Watchlists.
                AsNoTracking().
                Where(ut =>ut.UserId == userId).
                Include(ut => ut.Trademark).
                OrderByDescending(ut => ut.AddedOnUtc).
                Skip(skip).
                Take(take).
                ToListAsync(cancellationToken);
        }

        public async Task SoftRemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken)
        {
            var watchlist = await dbContext.Watchlists.
            FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId, 
                cancellationToken);

            if (watchlist is null) return;

            watchlist.IsDeleted = true;
            watchlist.NotificationsEnabled = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ToggleNotificationsAsync(
            string userId, 
            int trademarkId, 
            bool notificationsEnabled, 
            CancellationToken cancellationToken)
        {
            var watchlist = await dbContext.Watchlists.
                FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId && 
                !ut.IsDeleted, cancellationToken);

            if (watchlist is null) return;

            if (watchlist.NotificationsEnabled != notificationsEnabled)
            {
                watchlist.NotificationsEnabled = notificationsEnabled;

                dbContext.Entry(watchlist).
                    Property(x => x.NotificationsEnabled).
                    IsModified = true;
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
