using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Trademarks.Implementations
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
        /// Adds a user-trademark link if it does not exist, or undeletes it if it was previously soft-deleted.
        /// </summary>
        public async Task AddOrUndeleteAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            UserTrademark? collectionLink = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.TrademarkRegistrationId == trademarkId,
                    cancellationToken);

            if (collectionLink == null)
            {
                await dbContext.UserTrademarks.AddAsync(new UserTrademark()
                {
                    ApplicationUserId = userId,
                    TrademarkRegistrationId = trademarkId,
                    AddedToWatchlist = true,
                    DateAdded = DateTime.UtcNow,
                    IsDeleted = false
                }, cancellationToken);
            }

            else if (collectionLink.IsDeleted)
            {
                collectionLink.IsDeleted = false;
                collectionLink.AddedToWatchlist = true;
                collectionLink.DateAdded = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Checks whether a user is linked to a specific trademark.
        /// </summary>
        public Task<bool> IsLinkedAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken=default)
        {
            return dbContext.UserTrademarks.
                AnyAsync(ut =>ut.ApplicationUserId == userId &&
                ut.TrademarkRegistrationId == trademarkId &&(includeSoftDeleted || !ut.IsDeleted),cancellationToken);
        }

        /// <summary>
        /// Retrieves all trademarks currently linked to a user, excluding soft-deleted associations.
        /// </summary>
        public IQueryable<TrademarkEntity> QueryUserCollection(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Include(ut=>ut.TrademarkRegistration.Classes).
                Select(ut => ut.TrademarkRegistration).
                AsSplitQuery().
                AsNoTracking();
        }

        public IQueryable<UserTrademark> QueryUserLinks(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Include(ut => ut.TrademarkRegistration).
                ThenInclude(t => t.Classes).
                AsSplitQuery().
                AsNoTracking();
        }

        /// <summary>
        /// Marks the association between a user and a trademark as soft-deleted.
        /// Returns True if the link was active and is now soft-deleted, or false if it did not exist or was already deleted.
        /// </summary>
        public async Task<bool> SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            UserTrademark? userTrademarkLink = await dbContext.UserTrademarks
                .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId &&
                ut.TrademarkRegistrationId == trademarkId, cancellationToken);

            if (userTrademarkLink == null || userTrademarkLink.IsDeleted)
            {
                return false;
            }             

            userTrademarkLink.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
