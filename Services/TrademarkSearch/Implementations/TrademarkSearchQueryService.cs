using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.TrademarkSearch.Abstractions;
using IPNoticeHub.Application.TrademarkSearch.DTOs;
using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;

namespace IPNoticeHub.Application.TrademarkSearch.Implementations
{
    public sealed class TrademarkSearchQueryService : ITrademarkSearchQueryService
    {
        private readonly ITrademarkReadRepository tmSearchServiceRepo;

        public TrademarkSearchQueryService(ITrademarkReadRepository tmSearchServiceRepository)
        {
            this.tmSearchServiceRepo = tmSearchServiceRepository;
        }

        public async Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)>
            SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default)
        {
            var queryResult = tmSearchServiceRepo.TrademarkQueryNoTracking();

            var searchByFilter = searchQuery.SearchBy ?? TrademarkSearchBy.Wordmark;

            if (!string.IsNullOrWhiteSpace(searchQuery.Query))
            {
                string searchTerm = searchQuery.Query.Trim();

                bool isSearchModeIdentical = searchQuery.Mode == SearchMode.Identical;

                if (searchByFilter == TrademarkSearchBy.Number)
                {
                    if (isSearchModeIdentical)
                    {
                        queryResult = queryResult.Where(
                            te => te.RegistrationNumber == searchTerm);
                    }

                    else
                    {
                        queryResult = queryResult.Where(
                            te => EF.Functions.Like(te.RegistrationNumber, 
                            $"%{searchTerm}%"));
                    }
                }

                else if (searchByFilter == TrademarkSearchBy.Owner)
                {
                    if (isSearchModeIdentical)
                    {
                        queryResult = queryResult.Where(te => 
                        te.Owner == searchTerm);
                    }

                    else
                    {
                        queryResult = queryResult.Where(te => 
                        EF.Functions.Like(te.Owner, $"%{searchTerm}%"));
                    }
                }

                else
                {
                    if (isSearchModeIdentical)
                    {
                        queryResult = queryResult.Where(te => 
                        te.Wordmark == searchTerm);
                    }

                    else
                    {
                        queryResult = queryResult.Where(te => 
                        EF.Functions.Like(te.Wordmark, $"%{searchTerm}%"));
                    }
                }
            }

            if (searchQuery.Status.HasValue)
            {
                queryResult = queryResult.Where(t => 
                t.StatusCategory == searchQuery.Status.Value);
            }

            if (searchQuery.Class.HasValue)
            {
                queryResult = queryResult.Where(t => 
                t.Classes.Any(c => c.ClassNumber == 
                (int)searchQuery.Class.Value));
            }

            if (searchQuery.Office.HasValue)
            {
                queryResult = queryResult.Where(t => 
                t.Source == searchQuery.Office);
            }

            int skipItems = (searchQuery.Page - 1) * searchQuery.PageSize;

            int resultsCount = await queryResult.
                CountAsync(cancellationToken);

            var searchResults = await queryResult.
                OrderBy(t => t.RegistrationNumber).
                Skip(skipItems).
                Take(searchQuery.PageSize).
                Select(t => new TrademarkSearchResultDto
                {
                    Id = t.Id,
                    PublicId = t.PublicId,
                    RegistrationNumber = t.RegistrationNumber ?? string.Empty,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    Status = t.StatusCategory.ToString(),
                    RegistrationDate = t.RegistrationDate
                }).
                ToListAsync(cancellationToken);

            return (searchResults, resultsCount);
        }
    }
}
