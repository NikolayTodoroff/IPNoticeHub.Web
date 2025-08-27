using IPNoticeHub.Services.DTOs.Trademarks;

namespace IPNoticeHub.Services.Abstractions
{
    public interface ITrademarkSearchService
    {
        Task<PagedResult<TrademarkListItemDTO>> SearchAsync(TrademarkFilterDTO filter,int page,int pageSize);

        Task<TrademarkDetailsDTO?> GetDetailsAsync(Guid publicId);
    }
}
