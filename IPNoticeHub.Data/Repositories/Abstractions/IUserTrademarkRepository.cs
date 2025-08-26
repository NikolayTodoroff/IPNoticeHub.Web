using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.Abstractions
{
    public interface IUserTrademarkRepository
    {
        Task<bool> IsLinkedAsync(string userId, int trademarkId, bool includeSoftDeleted = false);

        Task AddOrUndeleteAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        Task<bool> SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the user's saved trademarks (joined), ready for projection.
        /// </summary>
        IQueryable<TrademarkEntity> QueryUserCollection(string userId);
    }
}
