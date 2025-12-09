using IPNoticeHub.Services.TrademarkSearch.DTOs;

namespace IPNoticeHub.Services.TrademarkSearch.Abstractions
{
    public interface ITrademarkSearchQueryService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)>
            SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default);
    }
}



