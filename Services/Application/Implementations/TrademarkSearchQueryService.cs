using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Services.Application.Implementations
{
    public sealed class TrademarkSearchQueryService : ITrademarkSearchQueryService
    {
        private readonly ITrademarkReadRepository tmSearchServiceRepo;

        public TrademarkSearchQueryService(ITrademarkReadRepository tmSearchServiceRepository)
        {
            this.tmSearchServiceRepo = tmSearchServiceRepository;
        }

        public async Task<(IReadOnlyList<TrademarkSearchResultDTO> Items, int Total)>
            SearchAsync(TrademarkSearchQueryDTO requestQuery, CancellationToken cancellationToken = default)
        {
            var query = tmSearchServiceRepo.TrademarkQueryNoTracking();

            var searchByFilter = requestQuery.SearchBy ?? TrademarkSearchBy.Wordmark;

            if (!string.IsNullOrWhiteSpace(requestQuery.Query))
            {
                string searchTerm = requestQuery.Query.Trim();
                bool isSearchModeIdentical = requestQuery.Mode == SearchMode.Identical;

                if (searchByFilter == TrademarkSearchBy.Number)
                {
                    if (isSearchModeIdentical)
                    {
                        query = query.Where(t => t.RegistrationNumber == searchTerm);
                    }
                    else
                    {
                        query = query.Where(t => EF.Functions.Like(t.RegistrationNumber, $"%{searchTerm}%"));
                    }
                }

                else if (searchByFilter == TrademarkSearchBy.Owner)
                {
                    if (isSearchModeIdentical)
                    {
                        query = query.Where(t => t.Owner == searchTerm);
                    }
                    else
                    {
                        query = query.Where(t => EF.Functions.Like(t.Owner, $"%{searchTerm}%"));
                    }
                }

                else
                {
                    if (isSearchModeIdentical)
                    {
                        query = query.Where(t => t.Wordmark == searchTerm);
                    }
                    else
                    {
                        query = query.Where(t => EF.Functions.Like(t.Wordmark, $"%{searchTerm}%"));
                    }
                }
            }

            if (requestQuery.Status.HasValue)
            {
                query = query.Where(t => t.StatusCategory == requestQuery.Status.Value);
            }

            if (requestQuery.Class.HasValue)
            {
                query = query.Where(t => t.Classes.Any(c => c.ClassNumber == (int)requestQuery.Class.Value));
            }

            if (requestQuery.Office.HasValue)
            {
                query = query.Where(t => t.Source == requestQuery.Office);
            }

            int skipItems = (requestQuery.Page - 1) * requestQuery.PageSize;

            int resultsCount = await query.CountAsync(cancellationToken);

            var searchResults = await query
                .OrderBy(t => t.RegistrationNumber)
                .Skip(skipItems).Take(requestQuery.PageSize)
                .Select(t => new TrademarkSearchResultDTO
                {
                    Id = t.Id,
                    PublicId = t.PublicId,
                    RegistrationNumber = t.RegistrationNumber ?? string.Empty,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    Status = t.StatusCategory.ToString(),
                    RegistrationDate = t.RegistrationDate
                })
                .ToListAsync(cancellationToken);

            return (searchResults, resultsCount);
        }
    }
}
