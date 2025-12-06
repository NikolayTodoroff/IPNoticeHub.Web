using IPNoticeHub.Services.Application.DTOs;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ITrademarkSearchQueryService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDTO> Items, int Total)>
            SearchAsync(TrademarkSearchQueryDTO searchQuery, CancellationToken cancellationToken = default);
    }
}



