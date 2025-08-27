using IPNoticeHub.Data.Repositories.Abstractions;
using IPNoticeHub.Services.Abstractions;
using IPNoticeHub.Services.DTOs.Trademarks;
using Microsoft.EntityFrameworkCore;
using static IPNoticeHub.Common.EntityValidationConstants;

namespace Services.Implementations
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
            var result = await trademarks.GetByPublicIdAsync(publicId, asNoTracking:true);

            if (result == null)
            {
                return null;
            }

            return new TrademarkDetailsDTO()
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

                Classes = result.Classes.
                    Select(c => c.ClassNumber).
                    ToList(),

                Events = result.Events.
                    OrderByDescending(e => e.EventDate).
                    Select(e => (Date: e.EventDate, Code: e.Code, Description: e.Description)).
                    ToList(),
            };
        }

        public async Task<PagedResult<TrademarkListItemDTO>> SearchAsync(TrademarkFilterDTO filter, int page, int pageSize)
        {
            // Normalize page & pageSize (apply defaults and upper limits)
            page = page < PagingConstants.DefaultPage ? PagingConstants.DefaultPage : page;

            if (pageSize < 1)
                pageSize = PagingConstants.DefaultPageSize;
            else if (pageSize > PagingConstants.MaxPageSize)
                pageSize = PagingConstants.MaxPageSize;

            var searchFilter = new TrademarkSearchFilter()
            {
                Provider = filter.Provider,
                ClassNumbers = filter.ClassNumbers,
                Status = filter.Status,
                SearchBy = filter.SearchBy,
                SearchTerm = filter.SearchTerm,
                ExactMatch = filter.ExactMatch
            };

            var queryResult = trademarks.Query(searchFilter,includeNav: true).
                OrderBy(t=>t.Wordmark);

            var totalResults = await queryResult.CountAsync();

            var items = await queryResult.
                Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                Items = items,
                Total = totalResults,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
