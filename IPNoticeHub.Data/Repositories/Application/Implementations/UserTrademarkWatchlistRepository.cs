using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Repositories.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Application.Implementations
{
    public class UserTrademarkWatchlistRepository : IUserTrademarkWatchlistRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserTrademarkWatchlistRepository(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddOrUndeleteAsync(string userId, int trademarkId, int? currentStatusCodeRaw,
            string? currentStatusText, DateTime? currentStatusDateUtc, CancellationToken cancellationToken)
        {
            var link = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == trademarkId, cancellationToken);

            if (link is null)
            {
                link = new UserTrademark
                {
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = trademarkId,
                    IsDeleted = false,

                    AddedToWatchlist = true,
                    WatchlistNotificationsEnabled = false,
                    WatchlistAddedOnUtc = DateTime.UtcNow,
                    WatchlistInitialStatusCodeRaw = currentStatusCodeRaw,
                    WatchlistInitialStatusText = currentStatusText,
                    WatchlistInitialStatusDateUtc = currentStatusDateUtc
                };

                await dbContext.UserTrademarks.AddAsync(link, cancellationToken);
            }

            else
            {
                if (link.IsDeleted)
                {
                    link.IsDeleted = false;
                }

                link.AddedToWatchlist = true;


                if (link.WatchlistAddedOnUtc is null)
                    link.WatchlistAddedOnUtc = DateTime.UtcNow;

                if (link.WatchlistInitialStatusCodeRaw is null)
                    link.WatchlistInitialStatusCodeRaw = currentStatusCodeRaw;

                if (string.IsNullOrEmpty(link.WatchlistInitialStatusText))
                    link.WatchlistInitialStatusText = currentStatusText;

                if (link.WatchlistInitialStatusDateUtc is null)
                    link.WatchlistInitialStatusDateUtc = currentStatusDateUtc;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CountByUserAsync(string userId, CancellationToken cancellationToken)
        {
            return await dbContext.UserTrademarks.CountAsync(ut => ut.ApplicationUserId == userId &&
                    !ut.IsDeleted && ut.AddedToWatchlist,  cancellationToken);
        }

        public async Task<bool> ExistsAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            return await dbContext.UserTrademarks.AnyAsync(ut => ut.ApplicationUserId == userId &&
                    ut.TrademarkRegistrationId == trademarkId && !ut.IsDeleted && ut.AddedToWatchlist, cancellationToken);
        }

        public async Task<IReadOnlyList<UserTrademark>> ListByUserAsync(string userId, int skip, int take, CancellationToken cancellationToken)
        {
            return await dbContext.UserTrademarks.
                AsNoTracking().
                Where(ut =>ut.ApplicationUserId == userId && !ut.IsDeleted && ut.AddedToWatchlist)
                .Include(ut => ut.TrademarkRegistration)
                .OrderByDescending(ut => ut.WatchlistAddedOnUtc)
                .ThenByDescending(ut => ut.DateAdded)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            var link = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId &&
                          ut.TrademarkRegistrationId == trademarkId && !ut.IsDeleted, cancellationToken);

            if (link is null)
            {
                return;
            }

            link.AddedToWatchlist = false;
            link.WatchlistNotificationsEnabled = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ToggleNotificationsAsync(string userId, int trademarkId, bool notificationsEnabled, CancellationToken cancellationToken)
        {
            var link = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == trademarkId &&
                        !ut.IsDeleted && ut.AddedToWatchlist, cancellationToken);

            if (link is null)
            {
                return;
            }

            if (link.WatchlistNotificationsEnabled != notificationsEnabled)
            {
                link.WatchlistNotificationsEnabled = notificationsEnabled;

                dbContext.Entry(link).Property(x => x.WatchlistNotificationsEnabled).IsModified = true;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
