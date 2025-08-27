using IPNoticeHub.Common.AdditionalConfigurations;
using IPNoticeHub.Data.Repositories.Abstractions;
using IPNoticeHub.Services.Abstractions;
using IPNoticeHub.Services.DTOs.Trademarks;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Implementations
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

            if (result is null)
            {
                return null;
            } 

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
                                        Select(e => (Date: e.EventDate, Code: e.Code, Description: e.Description)).
                                        ToList()
            };
        }

        public async Task<PagedResult<TrademarkListItemDTO>> SearchAsync(TrademarkFilterDTO filter, int page, int pageSize)
        {
            var (normalizedPage, normalizedPageSize) = PagingConfiguration.NormalizePaging(page, pageSize);

            var searchFilter = new TrademarkSearchFilter
            {
                Provider = filter.Provider,
                ClassNumbers = filter.ClassNumbers,
                Status = filter.Status,
                SearchBy = filter.SearchBy,
                SearchTerm = filter.SearchTerm,
                ExactMatch = filter.ExactMatch
            };

            var query = trademarks.Query(searchFilter, includeNav: true)
                                  .OrderBy(t => t.Wordmark);

            var resultsCount = await query.CountAsync();

            var searchResults = await query
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
                .ToListAsync();

            return new PagedResult<TrademarkListItemDTO>
            {
                Results = searchResults,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }
    }
}
