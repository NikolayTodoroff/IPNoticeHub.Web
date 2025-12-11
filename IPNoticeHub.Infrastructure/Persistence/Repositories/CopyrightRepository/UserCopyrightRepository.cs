using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Application.Repositories.CopyrightRepository;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository
{
    public sealed class UserCopyrightRepository : IUserCopyrightRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserCopyrightRepository(IPNoticeHubDbContext context) => dbContext = context;

        public async Task AddOrUndeleteAsync(
            string userId, 
            int copyrightId, 
            CancellationToken cancellationToken = default)
        {
            var userCopyright = 
                await dbContext.UserCopyrights.SingleOrDefaultAsync(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == copyrightId, 
                    cancellationToken);

            if (userCopyright is null)
            {
                userCopyright = new UserCopyright
                {
                    ApplicationUserId = userId,
                    CopyrightEntityId = copyrightId,
                    DateAdded = DateTime.UtcNow,
                    IsDeleted = false
                };

                await dbContext.UserCopyrights.AddAsync(
                    userCopyright, 
                    cancellationToken);
            }

            else if (userCopyright.IsDeleted)
            {
                userCopyright.IsDeleted = false;
                userCopyright.DateAdded = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsLinkedAsync(
            string userId, 
            int copyrightId, 
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = 
                dbContext.UserCopyrights.Where(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == copyrightId);

            if (!includeSoftDeleted)
            {
                query = query.Where(uc => !uc.IsDeleted);
            } 


            return query.
                AsNoTracking().
                AnyAsync(cancellationToken);
        }

        public IQueryable<CopyrightEntity> QueryUserCollection(string userId)
        {
            return dbContext.UserCopyrights.Where(
                uc => uc.ApplicationUserId == userId && !uc.IsDeleted).
                Select(uc => uc.CopyrightEntity).
                AsNoTracking();
        }       

        public IQueryable<UserCopyright> QueryUserLinks(string userId)
        {
            return dbContext.UserCopyrights.Where(
                uc => uc.ApplicationUserId == userId && !uc.IsDeleted).
                Include(uc => uc.CopyrightEntity).
                AsSplitQuery().
                AsNoTracking();
        }    

        public async Task<bool> SoftRemoveAsync(
            string userId, 
            int copyrightId, 
            CancellationToken cancellationToken = default)
        {
            var userCopyright = 
                await dbContext.UserCopyrights.SingleOrDefaultAsync(
                    uc => uc.ApplicationUserId == userId && 
                    uc.CopyrightEntityId == copyrightId, 
                    cancellationToken);

            if (userCopyright is null || userCopyright.IsDeleted)
            {
                return false;
            }
                

            userCopyright.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
