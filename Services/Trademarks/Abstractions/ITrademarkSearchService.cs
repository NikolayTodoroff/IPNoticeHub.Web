using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Abstractions
{
    public interface ITrademarkSearchService
    {
        Task<PagedResult<TrademarkListItemDTO>> SearchAsync(TrademarkFilterDTO filter,int page,int pageSize);

        Task<TrademarkDetailsDTO?> GetDetailsAsync(Guid publicId);
    }
}
