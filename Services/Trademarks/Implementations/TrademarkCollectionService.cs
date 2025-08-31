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
            var exists = await trademarks.ExistsAsync(trademarkId, cancellationToken);

            if (!exists) return;

            await userTrademarks.AddOrUndeleteAsync(userId, trademarkId, cancellationToken);
        }

        public async Task<PagedResult<TrademarkSummaryDTO>> GetUserCollectionAsync(string userId, int currentPage, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            IOrderedQueryable<TrademarkEntity>? userTrademarksQuery = userTrademarks.QueryUserCollection(userId).
                AsNoTracking().
                OrderByDescending(t => t.RegistrationDate.HasValue).
                ThenByDescending(t => t.RegistrationDate).
                ThenBy(t => t.Wordmark);

            int resultsCount = await userTrademarksQuery.CountAsync(cancellationToken);

            List<TrademarkSummaryDTO>? userTrademarksList = await userTrademarksQuery
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(t => new TrademarkSummaryDTO
                {
                    Id = t.Id,
                    PublicId = t.PublicId,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    SourceId = t.SourceId,
                    Status = t.StatusCategory,
                    Classes = t.Classes.Select(c => c.ClassNumber).ToArray(),
                    Provider = t.Source
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSummaryDTO>
            {
                Results = userTrademarksList,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public async Task<PagedResult<TrademarkSummaryDTO>> GetUserCollectionAsync(string userId, CollectionSortBy sortBy, int currentPage, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

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

            int resultsCount = await links.AsNoTracking().CountAsync(cancellationToken);

            List<TrademarkSummaryDTO>? results = await links.
                AsNoTracking().
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(l => new TrademarkSummaryDTO
                {
                   Id = l.TrademarkRegistrationId,
                   PublicId = l.TrademarkRegistration.PublicId,
                   Wordmark = l.TrademarkRegistration.Wordmark,
                   Owner = l.TrademarkRegistration.Owner,
                   SourceId = l.TrademarkRegistration.SourceId,
                   Status = l.TrademarkRegistration.StatusCategory,
                   Classes = l.TrademarkRegistration.Classes.Select(c => c.ClassNumber).ToArray(),
                   Provider = l.TrademarkRegistration.Source
                }).
                   ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSummaryDTO>
            {
                Results = results,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public Task<bool> IsInCollectionAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
        {
            return userTrademarks.IsLinkedAsync(userId, trademarkId, includeSoftDeleted, cancellationToken);
        }

        public async Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            await userTrademarks.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }
    }
}
