using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;
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

            if (!includeSoftDeleted) query = query.Where(uc => !uc.IsDeleted);

            return query.AsNoTracking().AnyAsync(cancellationToken);
        }

        public async Task<PagedResult<UserCopyright>> GetUserCollectionPageAsync(
            string userId,
            CollectionSortBy sortBy,
            int currentPage,
            int resultsPerPage,
            CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = 
                PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            IQueryable<UserCopyright> links = dbContext.UserCopyrights.
                Where(uc => uc.ApplicationUserId == userId && !uc.IsDeleted).
                Include(uc => uc.CopyrightEntity).
                AsNoTracking();

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                links = links.
                    OrderBy(l => l.DateAdded).
                    ThenBy(l => l.CopyrightEntityId);
            }

            else if (sortBy == CollectionSortBy.TitleAsc)
            {
                links = links.
                    OrderBy(l => l.CopyrightEntity.Title).
                    ThenBy(l => l.CopyrightEntityId);
            }

            else if (sortBy == CollectionSortBy.TitleDesc)
            {
                links = links.
                    OrderByDescending(l => l.CopyrightEntity.Title).
                    ThenBy(l => l.CopyrightEntityId);
            }

            else
            {
                links = links.
                    OrderByDescending(l => l.DateAdded).
                    ThenBy(l => l.CopyrightEntityId);
            }

            int resultsCount = await links.CountAsync(cancellationToken);

            List<UserCopyright> pageItems = await links.
                Skip((normalizedPage - 1) * normalizedPageSize).
                ToListAsync(cancellationToken);

            return new PagedResult<UserCopyright>
            {
                Results = pageItems,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
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

            if (userCopyright is null || userCopyright.IsDeleted) return false;

            userCopyright.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
