using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public interface IUserTrademarkRepository
    {
        Task<bool> IsLinkedAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken=default);

        Task AddOrUndeleteAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        Task<bool> SoftRemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default);

        IQueryable<TrademarkEntity> QueryUserCollection(string userId);

        IQueryable<UserTrademark> QueryUserLinks(string userId);
    }
}
