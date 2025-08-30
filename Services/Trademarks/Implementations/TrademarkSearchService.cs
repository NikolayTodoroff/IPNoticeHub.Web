using IPNoticeHub.Common.AdditionalConfigurations;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Trademarks.Implementations
{
    public sealed class TrademarkSearchService : ITrademarkSearchService
    {
        private readonly ITrademarkRepository trademarks;

        public TrademarkSearchService(ITrademarkRepository trademarks)
        {
            this.trademarks = trademarks;
        }

        public async Task<TrademarkDetailsDTO?> GetDetailsAsync(Guid publicId)
        {
            var result = await trademarks.GetByPublicIdAsync(publicId, asNoTracking: true);

            if (result is null) return null;

            return new TrademarkDetailsDTO
            {
                PublicId = result.PublicId,
                Wordmark = result.Wordmark,
                Owner = result.Owner,
                SourceId = result.SourceId,
                RegistrationNumber = result.RegistrationNumber,
                Status = result.StatusCategory,
                FilingDate = result.FilingDate,
                RegistrationDate = result.RegistrationDate,
                MarkImageUrl = result.MarkImageUrl,
                Provider = result.Source,
                Classes = result.Classes.Select(c => c.ClassNumber).ToList(),
                Events = result.Events.
                                        OrderByDescending(e => e.EventDate).
                                        Select(e => (Date: e.EventDate, e.Code, e.Description)).
                                        ToList()
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

            IOrderedQueryable<TrademarkEntity>? query = trademarks.Query(searchFilter, includeNav: true)
                                  .OrderBy(t => t.Wordmark);

            int resultsCount = await query.AsNoTracking().CountAsync(cancellationToken);

            List<TrademarkSummaryDTO>? searchResults = await query.
                AsNoTracking().
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
