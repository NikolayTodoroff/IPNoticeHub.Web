using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Trademarks.Implementations
{
    public class UserTrademarkRepository : IUserTrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserTrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public async Task AddOrUndeleteAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            UserTrademark? collectionLink = await dbContext.UserTrademarks.
                FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrademarkId == trademarkId,
                    cancellationToken);

            if (collectionLink == null)
            {
                await dbContext.UserTrademarks.AddAsync(new UserTrademark()
                {
                    UserId = userId,
                    TrademarkId = trademarkId,
                    DateAdded = DateTime.UtcNow,
                    IsDeleted = false
                }, cancellationToken);
            }

            else if (collectionLink.IsDeleted)
            {
                collectionLink.IsDeleted = false;
                collectionLink.DateAdded = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsLinkedAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken=default)
        {
            return dbContext.UserTrademarks.
                AnyAsync(ut =>ut.UserId == userId &&
                ut.TrademarkId == trademarkId &&(includeSoftDeleted || !ut.IsDeleted),cancellationToken);
        }

        public IQueryable<TrademarkEntity> QueryUserCollection(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.UserId == userId && !ut.IsDeleted).
                Include(ut=>ut.Trademark.Classes).
                Select(ut => ut.Trademark).
                AsSplitQuery().
                AsNoTracking();
        }

        public IQueryable<UserTrademark> QueryUserLinks(string userId)
        {
            return dbContext.UserTrademarks.
                Where(ut => ut.UserId == userId && !ut.IsDeleted).
                Include(ut => ut.Trademark).
                ThenInclude(t => t.Classes).
                AsSplitQuery().
                AsNoTracking();
        }

        public async Task<bool> SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            UserTrademark? userTrademarkLink = await dbContext.UserTrademarks
                .FirstOrDefaultAsync(ut => ut.UserId == userId &&
                ut.TrademarkId == trademarkId, cancellationToken);

            if (userTrademarkLink == null || userTrademarkLink.IsDeleted) return false;

            userTrademarkLink.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
