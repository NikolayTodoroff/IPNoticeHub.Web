using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Support;

namespace IPNoticeHub.Application.Repositories.TrademarkRepository;
public interface ITrademarkReadRepository
{
    IQueryable<TrademarkEntity> TrademarkQueryNoTracking();

    Task<PagedResult<TrademarkSearchResultDto>> SearchAsync(
        TrademarkSearchQueryDto searchQuery,
        CancellationToken cancellationToken = default);
} 
