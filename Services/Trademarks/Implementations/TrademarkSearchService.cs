using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;

namespace IPNoticeHub.Services.Trademarks.Implementations
{
    public sealed class TrademarkSearchService : ITrademarkSearchService
    {
        private readonly ITrademarkRepository trademarkRepository;

        public TrademarkSearchService(ITrademarkRepository trademarks)
        {
            this.trademarkRepository = trademarks;
        }

        public async Task<TrademarkDetailsDTO?> GetDetailsAsync(Guid publicId, CancellationToken cancellationToken = default)
        {
            TrademarkEntity? entity = await trademarkRepository.GetByPublicIdAsync(publicId, cancellationToken: cancellationToken);

            if (entity is null) return null;

            return new TrademarkDetailsDTO
            {
                PublicId = entity.PublicId,
                Wordmark = entity.Wordmark,
                Owner = entity.Owner,
                SourceId = entity.SourceId,
                RegistrationNumber = entity.RegistrationNumber,
                Status = entity.StatusCategory,
                GoodsAndServices = entity.GoodsAndServices,
                FilingDate = entity.FilingDate,
                RegistrationDate = entity.RegistrationDate,
                MarkImageUrl = entity.MarkImageUrl,
                Provider = entity.Source,
                Classes = entity.Classes.Select(c => c.ClassNumber).ToList(),
                Events = entity.Events.OrderByDescending(e => e.EventDate).
                     Select(e => (e.EventDate, e.Code, e.Description)).ToList()
            };
        }

        public async Task<PagedResult<TrademarkSummaryDTO>> SearchAsync(TrademarkFilterDTO filter, int currentPage, int resultsPerPage, CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(currentPage, resultsPerPage);

            TrademarkSearchFilter? searchFilter = new TrademarkSearchFilter
            {
                Provider = filter.Provider,
                ClassNumbers = filter.ClassNumbers,
                Status = filter.Status,
                SearchBy = filter.SearchBy,
                SearchTerm = filter.SearchTerm,
                ExactMatch = filter.ExactMatch
            };

            IOrderedQueryable<TrademarkEntity>? query = trademarkRepository.
                Query(searchFilter, includeNav: false).
                OrderBy(t => t.Wordmark).
                ThenBy(t => t.Id);

            int resultsCount = await query.CountAsync(cancellationToken);

            List<TrademarkSummaryDTO>? searchResults = await query.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(t => new TrademarkSummaryDTO
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
                Results = searchResults,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }
    }
}
