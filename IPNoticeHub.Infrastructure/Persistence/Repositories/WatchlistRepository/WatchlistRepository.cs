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
            var link = await dbContext.
                Watchlists.IgnoreQueryFilters().
                FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId, 
                cancellationToken);

            if (link is null)
            {
                link = new Watchlist
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
                    link, 
                    cancellationToken);
            }

            else
            {
                if (link.IsDeleted) link.IsDeleted = false;

                if (link.AddedOnUtc == default)
                    link.AddedOnUtc = DateTime.UtcNow;

                if (link.InitialStatusCodeRaw is null)
                    link.InitialStatusCodeRaw = currentStatusCodeRaw;

                if (string.IsNullOrEmpty(link.InitialStatusText))
                    link.InitialStatusText = currentStatusText;

                if (link.InitialStatusDateUtc is null)
                    link.InitialStatusDateUtc = currentStatusDateUtc;
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
            var link = await dbContext.Watchlists.
            FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId, 
                cancellationToken);

            if (link is null) return;

            link.IsDeleted = true;
            link.NotificationsEnabled = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ToggleNotificationsAsync(
            string userId, 
            int trademarkId, 
            bool notificationsEnabled, 
            CancellationToken cancellationToken)
        {
            var link = await dbContext.Watchlists.
                FirstOrDefaultAsync(
                ut => ut.UserId == userId && 
                ut.TrademarkId == trademarkId && 
                !ut.IsDeleted, cancellationToken);

            if (link is null) return;

            if (link.NotificationsEnabled != notificationsEnabled)
            {
                link.NotificationsEnabled = notificationsEnabled;

                dbContext.Entry(link).
                    Property(x => x.NotificationsEnabled).
                    IsModified = true;
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
