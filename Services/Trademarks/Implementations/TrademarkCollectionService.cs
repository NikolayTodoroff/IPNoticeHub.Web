using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Implementations
{
    public class TrademarkCollectionService : ITrademarkCollectionService
    {
        private readonly ITrademarkRepository trademarkRepository;
        private readonly IUserTrademarkRepository userTrademarkRepository;

        public TrademarkCollectionService(ITrademarkRepository trademarks, IUserTrademarkRepository userTrademarks)
        {
            this.trademarkRepository = trademarks;
            this.userTrademarkRepository = userTrademarks;
        }

        public async Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            bool linkExists = await trademarkRepository.ExistsAsync(trademarkId, cancellationToken);

            if (!linkExists) return;

            await userTrademarkRepository.AddOrUndeleteAsync(userId, trademarkId, cancellationToken);
        }

        public async Task<PagedResult<TrademarkSingleItemDto>> GetUserCollectionAsync(string userId, int currentPage, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            IOrderedQueryable<TrademarkEntity>? userTrademarksQuery = userTrademarkRepository.QueryUserCollection(userId).
                AsNoTracking().
                OrderByDescending(t => t.RegistrationDate.HasValue).
                ThenByDescending(t => t.RegistrationDate).
                ThenBy(t => t.Wordmark);

            int resultsCount = await userTrademarksQuery.CountAsync(cancellationToken);

            List<TrademarkSingleItemDto>? userTrademarksList = await userTrademarksQuery
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(t => new TrademarkSingleItemDto
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

            return new PagedResult<TrademarkSingleItemDto>
            {
                Results = userTrademarksList,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public async Task<PagedResult<TrademarkSingleItemDto>> GetUserCollectionAsync(string userId, CollectionSortBy sortBy, int currentPage, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            IQueryable<UserTrademark>? collectionLinks = userTrademarkRepository.QueryUserLinks(userId);

            if (sortBy == CollectionSortBy.DateAddedAsc)
            {
                collectionLinks = collectionLinks.OrderBy(l => l.DateAdded);
            }

            else if (sortBy == CollectionSortBy.WordmarkAsc)
            {
                collectionLinks = collectionLinks.OrderBy(l => l.Trademark.Wordmark);
            }

            else if (sortBy == CollectionSortBy.WordmarkDesc)
            {
                collectionLinks = collectionLinks.OrderByDescending(l => l.Trademark.Wordmark);
            }

            else
            {
                collectionLinks = collectionLinks.OrderByDescending(l => l.DateAdded);
            }

            int resultsCount = await collectionLinks.AsNoTracking().CountAsync(cancellationToken);

            List<TrademarkSingleItemDto>? results = await collectionLinks.
                AsNoTracking().
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(l => new TrademarkSingleItemDto
                {
                   Id = l.TrademarkId,
                   PublicId = l.Trademark.PublicId,
                   Wordmark = l.Trademark.Wordmark,
                   Owner = l.Trademark.Owner,
                   SourceId = l.Trademark.SourceId,
                   Status = l.Trademark.StatusCategory,
                   Classes = l.Trademark.Classes.Select(c => c.ClassNumber).ToArray(),
                   Provider = l.Trademark.Source
                }).
                   ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSingleItemDto>
            {
                Results = results,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }

        public Task<bool> IsInCollectionAsync(string userId, int trademarkId, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
        {
            return userTrademarkRepository.IsLinkedAsync(userId, trademarkId, includeSoftDeleted, cancellationToken);
        }

        public async Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken = default)
        {
            await userTrademarkRepository.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }
    }
}
