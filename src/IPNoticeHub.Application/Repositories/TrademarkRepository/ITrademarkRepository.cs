using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Support;

namespace IPNoticeHub.Application.Repositories.TrademarkRepository
{
    public interface ITrademarkRepository
    {
        IQueryable<TrademarkEntity> Query(
            TrademarkSearchFilter filter, 
            bool includeNav = false);

        Task<PagedResult<TrademarkSingleItemDto>> GetSearchPageAsync(
            TrademarkSearchFilter filter,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

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
