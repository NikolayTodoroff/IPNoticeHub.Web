using IPNoticeHub.Application.TrademarkSearch.DTOs;

namespace IPNoticeHub.Application.TrademarkSearch.Abstractions
{
    public interface ITrademarkSearchQueryService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)>
            SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default);
    }
}



