using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;

namespace IPNoticeHub.Application.Repositories.TrademarkRepository
{
    public interface IUserTrademarkRepository
    {
        Task<bool> IsLinkedAsync(
            string userId, 
            int trademarkId, 
            bool includeSoftDeleted = false, 
            CancellationToken cancellationToken=default);

        Task AddOrUndeleteAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default);

        Task<bool> SoftRemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default);

        Task<PagedResult<UserTrademark>> GetUserCollectionPageAsync(
            string userId,
            CollectionSortBy sortBy,
            int currentPage,
            int resultsPerPage,
            CancellationToken cancellationToken = default);

        IQueryable<UserTrademark> GetUserLinks(
            string userId);
    }
}
