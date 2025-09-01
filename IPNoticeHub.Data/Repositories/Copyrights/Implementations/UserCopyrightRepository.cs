using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Repositories.Copyrights.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Copyrights.Implementations
{
    public sealed class UserCopyrightRepository : IUserCopyrightRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserCopyrightRepository(IPNoticeHubDbContext context) => dbContext = context;

        public async Task AddOrUndeleteAsync(string userId, int copyrightId, CancellationToken cancellationToken = default)
        {
            UserCopyright? collectionLink = await dbContext.UserCopyrights
                .SingleOrDefaultAsync(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightId, cancellationToken);

            if (collectionLink is null)
            {
                collectionLink = new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightRegistrationId = copyrightId,
                    DateAdded = DateTime.UtcNow,
                    IsDeleted = false
                };

                await dbContext.UserCopyrights.AddAsync(collectionLink, cancellationToken);
            }

            else if (collectionLink.IsDeleted)
            {
                collectionLink.IsDeleted = false;
                collectionLink.DateAdded = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsLinkedAsync(string userId, int copyrightId, bool includeSoftDeleted = false,CancellationToken cancellationToken = default)
        {
            var query = dbContext.UserCopyrights
                .Where(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightId);

            if (!includeSoftDeleted)
                query = query.Where(uc => !uc.IsDeleted);

            return query.AsNoTracking().AnyAsync(cancellationToken);
        }

        public IQueryable<CopyrightEntity> QueryUserCollection(string userId)
        {
            return dbContext.UserCopyrights
                .Where(uc => uc.ApplicationUserId == userId && !uc.IsDeleted)
                .Select(uc => uc.CopyrightRegistration)
                .AsNoTracking();
        }       

        public IQueryable<UserCopyright> QueryUserLinks(string userId)
        {
            return dbContext.UserCopyrights.
                Where(uc => uc.ApplicationUserId == userId && !uc.IsDeleted).
                Include(uc => uc.CopyrightRegistration).
                AsSplitQuery().
                AsNoTracking();
        }    

        public async Task<bool> SoftRemoveAsync(string userId, int copyrightId, CancellationToken cancellationToken = default)
        {
            var link = await dbContext.UserCopyrights
                .SingleOrDefaultAsync(uc => uc.ApplicationUserId == userId && uc.CopyrightRegistrationId == copyrightId, cancellationToken);

            if (link is null || link.IsDeleted)
                return false;

            link.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
