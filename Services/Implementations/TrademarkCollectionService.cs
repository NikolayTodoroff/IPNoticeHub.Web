using IPNoticeHub.Common.AdditionalConfigurations;
using IPNoticeHub.Data.Repositories.Abstractions;
using IPNoticeHub.Services.Abstractions;
using IPNoticeHub.Services.DTOs.Trademarks;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Implementations
{
    public class TrademarkCollectionService : ITrademarkCollectionService
    {
        private readonly ITrademarkRepository trademarks;
        private readonly IUserTrademarkRepository userTrademarks;

        public TrademarkCollectionService(ITrademarkRepository trademarks, IUserTrademarkRepository userTrademarks)
        {
            this.trademarks = trademarks;
            this.userTrademarks = userTrademarks;
        }

        public async Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            var exists = await trademarks.ExistsAsync(trademarkId);
            if (!exists) return;

            await userTrademarks.AddOrUndeleteAsync(userId, trademarkId, cancellationToken);
        }

        public async Task<PagedResult<TrademarkListItemDTO>> GetUserCollectionAsync(
    string userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(page, pageSize);

            var userTrademarksQuery = userTrademarks.QueryUserCollection(userId)
                .OrderByDescending(t => t.RegistrationDate)
                .ThenBy(t => t.Wordmark);

            var resultsCount = await userTrademarksQuery.CountAsync(cancellationToken);

            var userTrademarksList = await userTrademarksQuery
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(t => new TrademarkListItemDTO
                {
                    PublicId = t.PublicId,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    SourceId = t.SourceId,
                    Status = t.StatusCategory,
                    Classes = t.Classes.Select(c => c.ClassNumber).ToArray(),
                    Provider = t.Source
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<TrademarkListItemDTO>
            {
                Results = userTrademarksList,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public Task<bool> IsInCollectionAsync(string userId, int trademarkId, bool includeSoftDeleted = false)
        {
            return userTrademarks.IsLinkedAsync(userId, trademarkId, includeSoftDeleted);
        }

        public async Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            await userTrademarks.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }
    }
}
