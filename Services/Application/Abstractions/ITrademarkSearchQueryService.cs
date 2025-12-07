using IPNoticeHub.Services.Application.DTOs;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ITrademarkSearchQueryService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)>
            SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default);
    }
}



