using IPNoticeHub.Services.Application.DTOs;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ITrademarkSearchService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDTO> Items, int Total)> 
            SearchAsync(TrademarkSearchQuery query, CancellationToken cancellationToken = default);
    }
}



