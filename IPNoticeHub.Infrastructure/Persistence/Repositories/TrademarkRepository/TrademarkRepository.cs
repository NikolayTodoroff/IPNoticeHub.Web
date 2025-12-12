using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Data;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Support;
using Microsoft.EntityFrameworkCore;
using static IPNoticeHub.Shared.Support.SearchNumberNormalizer;


namespace IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository
{
    public class TrademarkRepository : ITrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public TrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public Task<bool> ExistsAsync(
            int id, 
            CancellationToken cancellationToken)
        {
            return dbContext.TrademarkRegistrations.
                AsNoTracking().
                AnyAsync(
                t => t.Id == id, 
                cancellationToken);
        }

        public async Task<TrademarkEntity?> GetByPublicIdAsync(
            Guid publicId, 
            bool asNoTracking = true, 
            CancellationToken cancellationToken=default)
        {
            var trademarksQuery = dbContext.TrademarkRegistrations.
               Include(t => t.Classes).
               Include(t => t.Events).
               AsSplitQuery();

            if (asNoTracking) trademarksQuery = 
                    trademarksQuery.AsNoTracking();

            return await trademarksQuery.SingleOrDefaultAsync(
                t => t.PublicId == publicId, 
                cancellationToken);
        }

        public Task<int?> GetIdByPublicIdAsync(
            Guid publicId, 
            CancellationToken cancellationToken)
        {
            return dbContext.TrademarkRegistrations.
                AsNoTracking().
                Where(t => t.PublicId == publicId).
                Select(t => (int?)t.Id).
                FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<TrademarkEntity> Query(
            TrademarkSearchFilter filter, 
            bool includeNav = false)
        {
            IQueryable<TrademarkEntity> trademarksQuery = 
                dbContext.TrademarkRegistrations;

            if (includeNav)
            {
                trademarksQuery = trademarksQuery.
                    Include(t => t.Classes).
                    Include(t => t.Events).
                    AsSplitQuery();
            }

            if (filter.Provider.HasValue)
            {
                trademarksQuery = trademarksQuery.
                    Where(t => t.Source == filter.Provider.Value);
            }

            if (filter.Status.HasValue)
            {
                trademarksQuery = trademarksQuery.
                    Where(t => t.StatusCategory == filter.Status.Value);
            }

            if (filter.ClassNumbers != null && filter.ClassNumbers.Length > 0)
            {
                trademarksQuery = trademarksQuery.
                    Where(t => t.Classes
                    .Any(c => filter.ClassNumbers.
                    Contains(c.ClassNumber)));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                string searchTerm = filter.SearchTerm.Trim().
                    ToUpperInvariant();

                bool exact = filter.ExactMatch;

                if (filter.SearchBy == TrademarkSearchBy.Wordmark)
                {
                    trademarksQuery = exact ? trademarksQuery.Where(
                        t => (t.Wordmark ?? "").ToUpper() == searchTerm) : 
                        trademarksQuery.Where(
                            t => (t.Wordmark ?? "").
                            ToUpper().
                            Contains(searchTerm));
                }

                else if (filter.SearchBy == TrademarkSearchBy.Owner)
                {
                    trademarksQuery = exact ? trademarksQuery.Where( 
                        t => (t.Owner ?? "").ToUpper() == searchTerm) : 
                        trademarksQuery.Where(
                            t => (t.Owner ?? "").ToUpper().Contains(searchTerm));
                }

                else if (filter.SearchBy == TrademarkSearchBy.Number)
                {
                    string normalizedSearchTerm = NormalizeSearchNumber(filter.SearchTerm);
                 
                        if (filter.ExactMatch)
                        {
                            trademarksQuery = ApplyNormalizedSearchFilter(
                                trademarksQuery, 
                                normalizedSearchTerm, 
                                true);
                        }

                        else
                        {
                            trademarksQuery = ApplyNormalizedSearchFilter(
                                trademarksQuery, 
                                normalizedSearchTerm, 
                                false);
                        }                     
                }
            }

            return trademarksQuery.AsNoTracking();
        }

        public async Task<PagedResult<TrademarkSingleItemDto>> GetSearchPageAsync(
            TrademarkSearchFilter filter,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TrademarkEntity> query = Query(filter, includeNav: false).
                OrderBy(t => t.Wordmark).
                ThenBy(t => t.Id);

            int resultsCount = await query.CountAsync(cancellationToken);

            List<TrademarkSingleItemDto> searchResults = await query.
                Skip((page - 1) * pageSize).
                Take(pageSize).
                Select(t => new TrademarkSingleItemDto
                {
                    Id = t.Id,
                    PublicId = t.PublicId,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    SourceId = t.SourceId,
                    Status = t.StatusCategory,
                    Classes = t.Classes.
                    Select(c => c.ClassNumber).
                    ToArray(),
                    Provider = t.Source
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSingleItemDto>
            {
                Results = searchResults,
                ResultsCount = resultsCount,
                CurrentPage = page,
                ResultsCountPerPage = pageSize
            };
        }

        private static IQueryable<TrademarkEntity> ApplyNormalizedSearchFilter(
            IQueryable<TrademarkEntity> query,
            string normalizedSearchTerm, 
            bool exactMatch)
        {
            if (exactMatch)
            {
                return query.Where(trademark =>
                    ((trademark.RegistrationNumber ?? "")
                        .Replace("-", "")
                        .Replace("/", "")
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace("_", "")
                        .ToUpper()) == normalizedSearchTerm

                    ||

                    ((trademark.SourceId ?? "")
                        .Replace("-", "")
                        .Replace("/", "")
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace("_", "")
                        .ToUpper()) == normalizedSearchTerm
                );
            }

            else
            {
                return query.Where(trademark =>
                    ((trademark.RegistrationNumber ?? "")
                        .Replace("-", "")
                        .Replace("/", "")
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace("_", "")
                        .ToUpper()).Contains(normalizedSearchTerm)

                    ||

                    ((trademark.SourceId ?? "")
                        .Replace("-", "")
                        .Replace("/", "")
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace("_", "")
                        .ToUpper()).Contains(normalizedSearchTerm)
                );
            }
        }
    }
}
