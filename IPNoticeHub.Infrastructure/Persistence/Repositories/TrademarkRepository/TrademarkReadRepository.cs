using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository
{
    public sealed class TrademarkReadRepository : ITrademarkReadRepository
    {
        private readonly IPNoticeHubDbContext dbContext;
        public TrademarkReadRepository(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
      
        public IQueryable<TrademarkEntity> TrademarkQueryNoTracking()
        {
            return dbContext.TrademarkRegistrations.
              Include(t => t.Classes).
              AsNoTracking();
        }

        public async Task<PagedResult<TrademarkSearchResultDto>> SearchAsync(
        TrademarkSearchQueryDto searchQuery,
        CancellationToken cancellationToken = default)
        {
            IQueryable<TrademarkEntity> queryResult = TrademarkQueryNoTracking();

            var searchByFilter = 
                searchQuery.SearchBy ?? TrademarkSearchBy.Wordmark;

            if (!string.IsNullOrWhiteSpace(searchQuery.Query))
            {
                string searchTerm = searchQuery.Query.Trim();
                bool isSearchModeIdentical = searchQuery.Mode == SearchMode.Identical;

                if (searchByFilter == TrademarkSearchBy.Number)
                {
                    queryResult = isSearchModeIdentical ? 
                        queryResult.Where(te => te.RegistrationNumber == searchTerm) : 
                        queryResult.Where(te => EF.Functions.Like(
                            te.RegistrationNumber, $"%{searchTerm}%"));
                }
                else if (searchByFilter == TrademarkSearchBy.Owner)
                {
                    queryResult = isSearchModeIdentical ? 
                        queryResult.Where(te => te.Owner == searchTerm) : 
                        queryResult.Where(te => EF.Functions.Like(
                            te.Owner, $"%{searchTerm}%"));
                }
                else 
                {
                    queryResult = isSearchModeIdentical ? 
                        queryResult.Where(te => te.Wordmark == searchTerm) : 
                        queryResult.Where(te => EF.Functions.Like(
                            te.Wordmark, $"%{searchTerm}%"));
                }
            }

            if (searchQuery.Status.HasValue)
            {
                queryResult = queryResult.Where(te =>
                    te.StatusCategory == searchQuery.Status.Value);
            }

            if (searchQuery.Class.HasValue)
            {
                int classNumber = (int)searchQuery.Class.Value;
                queryResult = queryResult.Where(te =>
                    te.Classes.Any(c => c.ClassNumber == classNumber));
            }

            if (searchQuery.Office.HasValue)
            {
                queryResult = queryResult.Where(te =>
                    te.Source == searchQuery.Office.Value);
            }

            int skipItems = (searchQuery.Page - 1) * searchQuery.PageSize;

            int resultsCount = await queryResult.CountAsync(cancellationToken);

            var searchResults = await queryResult.
                OrderBy(te => te.RegistrationNumber).
                Skip(skipItems).
                Take(searchQuery.PageSize).
                Select(te => new TrademarkSearchResultDto
                {
                    Id = te.Id,
                    PublicId = te.PublicId,
                    RegistrationNumber = te.RegistrationNumber ?? string.Empty,
                    Wordmark = te.Wordmark,
                    Owner = te.Owner,
                    Status = te.StatusCategory.ToString(),
                    RegistrationDate = te.RegistrationDate
                }).
                ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSearchResultDto>
            {
                Results = searchResults,
                ResultsCount = resultsCount,
                CurrentPage = searchQuery.Page,
                ResultsCountPerPage = searchQuery.PageSize
            };
        }
    }
}
