using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public interface ITrademarkRepository
    {
        IQueryable<TrademarkEntity> Query(TrademarkSearchFilter filter, bool includeNav = false);

        Task<TrademarkEntity?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true);

        Task<int?> GetIdByPublicIdAsync(Guid publicId);

        Task<bool> ExistsAsync(int id);

    }
}
