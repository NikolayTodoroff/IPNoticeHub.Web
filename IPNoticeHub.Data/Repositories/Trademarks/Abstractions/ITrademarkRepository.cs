using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public interface ITrademarkRepository
    {
        IQueryable<TrademarkEntity> Query(TrademarkSearchFilter filter, bool includeNav = false);

        Task<TrademarkEntity?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking = true);

        Task<int?> GetIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    }
}
