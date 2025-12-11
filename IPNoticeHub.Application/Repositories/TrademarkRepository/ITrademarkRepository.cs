using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Application.Repositories.TrademarkRepository
{
    public interface ITrademarkRepository
    {
        IQueryable<TrademarkEntity> Query(
            TrademarkSearchFilter filter, 
            bool includeNav = false);

        Task<TrademarkEntity?> GetByPublicIdAsync(
            Guid publicId, 
            bool asNoTracking = true, 
            CancellationToken cancellationToken=default);

        Task<int?> GetIdByPublicIdAsync(
            Guid publicId, 
            CancellationToken cancellationToken);

        Task<bool> ExistsAsync(
            int id, 
            CancellationToken cancellationToken);
    }
}
