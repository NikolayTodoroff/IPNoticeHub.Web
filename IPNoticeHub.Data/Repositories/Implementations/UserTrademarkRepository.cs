using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository for managing associations between users and trademark registrations.
    /// Provides methods for linking, querying, and soft-deleting user-trademark relationships.
    /// </summary>
    public class UserTrademarkRepository : IUserTrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserTrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        /// Adds a user-trademark link if it does not exist,
        /// or undeletes it if it was previously soft-deleted.
        /// </summary>
        public async Task AddOrUndeleteAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            var link = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId &&
                ut.TrademarkRegistrationId == trademarkId, cancellationToken);

            if (link == null)
            {
                await dbContext.UserTrademarks.AddAsync(new UserTrademark
                {
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = trademarkId,
                    AddedToWatchlist = true,
                    DateAdded = DateTime.UtcNow,
                    IsDeleted = false
                }, cancellationToken);
            }

            else if (link.IsDeleted)
            {
                link.IsDeleted = false;
                link.AddedToWatchlist = true;
                link.DateAdded = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Checks whether a user is linked to a specific trademark.
        /// </summary>
        public Task<bool> IsLinkedAsync(string userId, int trademarkId, bool includeSoftDeleted = false)
        {
            return dbContext.UserTrademarks.AnyAsync(ut =>
                ut.ApplicationUserId == userId &&
                ut.TrademarkRegistrationId == trademarkId &&
                (includeSoftDeleted || !ut.IsDeleted));
        }

        /// <summary>
        /// Retrieves all trademarks currently linked to a user,
        /// excluding soft-deleted associations.
        /// </summary>
        public IQueryable<TrademarkEntity> QueryUserCollection(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Select(ut => ut.TrademarkRegistration).
                Include(t => t.Classes).
                AsNoTracking();
        }

        public IQueryable<UserTrademark> QueryUserLinks(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Include(ut => ut.TrademarkRegistration).
                ThenInclude(t => t.Classes).
                AsNoTracking();
        }

        /// <summary>
        /// Marks the association between a user and a trademark as soft-deleted.
        /// </summary>
        /// True if the link was active and is now soft-deleted,
        /// or false if it did not exist or was already deleted.
        /// </returns>
        public async Task<bool> SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            var link = await dbContext.UserTrademarks
                .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId &&
                ut.TrademarkRegistrationId == trademarkId, cancellationToken);

            if (link == null || link.IsDeleted)
            {
                return false;
            }             

            link.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
