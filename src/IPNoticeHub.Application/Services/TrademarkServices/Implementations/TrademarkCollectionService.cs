using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Application.Services.TrademarkService.Implementations
{
    public class TrademarkCollectionService : ITrademarkCollectionService
    {
        private readonly ITrademarkRepository trademarkRepository;
        private readonly IUserTrademarkRepository userTrademarkRepository;

        public TrademarkCollectionService(
            ITrademarkRepository trademarks, 
            IUserTrademarkRepository userTrademarks)
        {
            this.trademarkRepository = trademarks;
            this.userTrademarkRepository = userTrademarks;
        }

        public async Task AddAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default)
        {
            bool linkExists = await trademarkRepository.ExistsAsync(trademarkId, cancellationToken);
            if (!linkExists) return;

            await userTrademarkRepository.AddOrUndeleteAsync(userId, trademarkId, cancellationToken);
        }

        public Task<PagedResult<TrademarkSingleItemDto>> GetUserCollectionAsync(
                string userId,
                int currentPage,
                int resultsPerPage,
                CancellationToken cancellationToken = default)
        {
            const CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc;

            return GetUserCollectionAsync(
                userId,
                sortBy,
                currentPage,
                resultsPerPage,
                cancellationToken);
        }

        public async Task<PagedResult<TrademarkSingleItemDto>>GetUserCollectionAsync(
            string userId, 
            CollectionSortBy sortBy, 
            int currentPage, 
            int resultsPerPage, 
            CancellationToken cancellationToken = default)
        {
            var pagedResult = 
                await userTrademarkRepository.GetUserCollectionPageAsync(
                userId,
                sortBy,
                currentPage,
                resultsPerPage,
                cancellationToken);

            var mappedResults = pagedResult.Results.
                Select(ut => new TrademarkSingleItemDto
                {
                    Id = ut.TrademarkEntityId,
                    PublicId = ut.TrademarkEntity.PublicId,
                    Wordmark = ut.TrademarkEntity.Wordmark,
                    Owner = ut.TrademarkEntity.Owner,
                    SourceId = ut.TrademarkEntity.SourceId,
                    Status = ut.TrademarkEntity.StatusCategory,
                    Classes = ut.TrademarkEntity.Classes.
                    Select(c => c.ClassNumber).ToArray(),
                    Provider = ut.TrademarkEntity.Source
                }).
                ToList();
         
            return new PagedResult<TrademarkSingleItemDto>
            {
                Results = mappedResults,
                ResultsCount = pagedResult.ResultsCount,
                CurrentPage = pagedResult.CurrentPage,
                ResultsCountPerPage = pagedResult.ResultsCountPerPage
            };
        }

        public Task<bool>IsInCollectionAsync(
            string userId, 
            int trademarkId, 
            bool includeSoftDeleted = false, 
            CancellationToken cancellationToken = default)
        {
            return userTrademarkRepository.IsLinkedAsync(userId, trademarkId, includeSoftDeleted, cancellationToken);
        }

        public async Task RemoveAsync(
            string userId, 
            int trademarkId, 
            CancellationToken cancellationToken = default)
        {
            await userTrademarkRepository.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }
    }
}
