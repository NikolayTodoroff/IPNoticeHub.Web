using IPNoticeHub.Common.AdditionalConfigurations;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Trademarks.Implementations
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

        public async Task<PagedResult<TrademarkListItemDTO>> GetUserCollectionAsync(string userId, int page, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(page, resultsPerPage);

            IOrderedQueryable<TrademarkEntity>? userTrademarksQuery = userTrademarks.QueryUserCollection(userId).
                OrderByDescending(t => t.RegistrationDate.HasValue).
                ThenByDescending(t => t.RegistrationDate).
                ThenBy(t => t.Wordmark);

            int resultsCount = await userTrademarksQuery.CountAsync(cancellationToken);

            List<TrademarkListItemDTO>? userTrademarksList = await userTrademarksQuery
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

        public async Task<PagedResult<TrademarkListItemDTO>> GetUserCollectionAsync(string userId, CollectionSortBy sortBy, int page, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(page, resultsPerPage);

            IQueryable<UserTrademark>? links = userTrademarks.QueryUserLinks(userId);

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                links = links.OrderBy(l => l.DateAdded);
            }

            else if (sortBy == CollectionSortBy.WordmarkAsc)
            {
                links = links.OrderBy(l => l.TrademarkRegistration.Wordmark);
            }

            else if (sortBy == CollectionSortBy.WordmarkDesc)
            {
                links = links.OrderByDescending(l => l.TrademarkRegistration.Wordmark);
            }

            else
            {
                links = links.OrderByDescending(l => l.DateAdded);
            }

            int resultsCount = await links.CountAsync(cancellationToken);

            List<TrademarkListItemDTO>? results = await links.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(l => new TrademarkListItemDTO
                {
                   PublicId = l.TrademarkRegistration.PublicId,
                   Wordmark = l.TrademarkRegistration.Wordmark,
                   Owner = l.TrademarkRegistration.Owner,
                   SourceId = l.TrademarkRegistration.SourceId,
                   Status = l.TrademarkRegistration.StatusCategory,
                   Classes = l.TrademarkRegistration.Classes.Select(c => c.ClassNumber).ToArray(),
                   Provider = l.TrademarkRegistration.Source
                }).
                   ToListAsync(cancellationToken);

            return new PagedResult<TrademarkListItemDTO>
            {
                Results = results,
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
