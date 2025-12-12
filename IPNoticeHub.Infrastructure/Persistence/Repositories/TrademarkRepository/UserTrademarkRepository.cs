using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository
{
    public class UserTrademarkRepository : IUserTrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public UserTrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public async Task AddOrUndeleteAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default)
        {
            UserTrademark? collectionLink = 
                await dbContext.UserTrademarks.FirstOrDefaultAsync(
                    ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == trademarkId,
                    cancellationToken);

            if (collectionLink == null)
            {
                await dbContext.UserTrademarks.AddAsync(new UserTrademark()
                {
                    ApplicationUserId = userId,
                    TrademarkEntityId = trademarkId,
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

        public Task<bool> IsLinkedAsync(
            string userId, 
            int trademarkId, 
            bool includeSoftDeleted = false, 
            CancellationToken cancellationToken=default)
        {
            return dbContext.UserTrademarks.AnyAsync(
                ut => ut.ApplicationUserId == userId &&
                ut.TrademarkEntityId == trademarkId && 
                (includeSoftDeleted || !ut.IsDeleted),
                cancellationToken);
        }

        public async Task<PagedResult<UserTrademark>> GetUserCollectionPageAsync(
            string userId,
            CollectionSortBy sortBy,
            int currentPage,
            int resultsPerPage,
            CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) =
                PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            IQueryable<UserTrademark> links = dbContext.UserTrademarks.
                Where(ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Include(ut => ut.TrademarkEntity).
                ThenInclude(t => t.Classes).
                AsSplitQuery().
                AsNoTracking();

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                links = links.OrderBy(l => l.DateAdded);
            }

            else if (sortBy == CollectionSortBy.WordmarkAsc)
            {
                links = links.OrderBy(l => l.TrademarkEntity.Wordmark);
            }

            else if (sortBy == CollectionSortBy.WordmarkDesc)
            {
                links = links.OrderByDescending(l => l.TrademarkEntity.Wordmark);
            }

            else
            {
                links = links.OrderByDescending(l => l.DateAdded);
            }

            int resultsCount = await links.CountAsync(cancellationToken);

            List<UserTrademark> pagedItems = await links.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                ToListAsync(cancellationToken);

            return new PagedResult<UserTrademark>
            {
                Results = pagedItems,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public IQueryable<UserTrademark> QueryUserLinks(string userId)
        {
            return dbContext.UserTrademarks.Where(
                ut => ut.ApplicationUserId == userId && !ut.IsDeleted).
                Include(ut => ut.TrademarkEntity).
                ThenInclude(t => t.Classes).
                AsSplitQuery().
                AsNoTracking();
        }

        public async Task<bool> SoftRemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default)
        {
            UserTrademark? userTrademarkLink = 
                await dbContext.UserTrademarks.FirstOrDefaultAsync(
                    ut => ut.ApplicationUserId == userId && 
                    ut.TrademarkEntityId == trademarkId,
                    cancellationToken);

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
