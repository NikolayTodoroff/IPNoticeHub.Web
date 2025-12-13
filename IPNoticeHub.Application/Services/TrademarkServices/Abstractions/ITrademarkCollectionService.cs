using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Application.Services.TrademarkService.Abstractions
{
    public interface ITrademarkCollectionService
    {
        Task AddAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default);

        Task RemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default);

        Task<bool> IsInCollectionAsync(
            string userId, 
            int trademarkId, 
            bool includeSoftDeleted = false, 
            CancellationToken cancellationToken=default);
      
        Task<PagedResult<TrademarkSingleItemDto>>GetUserCollectionAsync(
            string userId,
            int currentPage,
            int resultsPerPage,
            CancellationToken cancellationToken = default);

        Task<PagedResult<TrademarkSingleItemDto>>GetUserCollectionAsync(string userId, 
           CollectionSortBy sortBy,
           int page, 
           int resultsPerPage, 
           CancellationToken ct = default);
    }
}
