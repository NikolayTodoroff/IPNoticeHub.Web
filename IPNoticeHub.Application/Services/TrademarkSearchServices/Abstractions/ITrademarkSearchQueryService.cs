using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Application.Services.TrademarkSearchService.Abstractions
{
    public interface ITrademarkSearchQueryService
    {
        Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)>
            SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default);
    }
}



