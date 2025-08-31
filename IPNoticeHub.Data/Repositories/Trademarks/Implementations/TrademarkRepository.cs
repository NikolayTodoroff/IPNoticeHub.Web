using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Trademarks.Implementations
{
    public class TrademarkRepository : ITrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public TrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            return dbContext.TrademarkRegistrations.
                AsNoTracking().
                AnyAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<TrademarkEntity?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking = true)
        {
            var trademarksQuery = dbContext.TrademarkRegistrations.
               Include(t => t.Classes).
               Include(t => t.Events).
               AsSplitQuery();

            if (asNoTracking)
            {
                trademarksQuery = trademarksQuery.AsNoTracking();
            }

            return await trademarksQuery.SingleOrDefaultAsync(t => t.PublicId == publicId, cancellationToken);
        }

        public Task<int?> GetIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            return dbContext.TrademarkRegistrations.
                AsNoTracking().
                Where(t=>t.PublicId == publicId).
                Select(t=>(int?)t.Id).
                FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Queries the TrademarkEntity table based on the provided filter and optional navigation properties.
        /// </summary>
        public IQueryable<TrademarkEntity> Query(TrademarkSearchFilter filter, bool includeNav = false)
        {
            IQueryable<TrademarkEntity> trademarksQuery = dbContext.TrademarkRegistrations;

            if (includeNav)
            {
                trademarksQuery = trademarksQuery.
                    Include(t => t.Classes).
                    Include(t => t.Events).
                    AsSplitQuery(); 
            }

            if (filter.Provider.HasValue)
            {
                trademarksQuery = trademarksQuery.Where(t => t.Source == filter.Provider.Value);
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
                    .Any(c => filter.ClassNumbers.Contains(c.ClassNumber)));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                string searchTerm = filter.SearchTerm.Trim();
                bool exact = filter.ExactMatch;

                if (filter.SearchBy == TrademarkSearchBy.Wordmark)
                {
                     trademarksQuery = exact ?
                        trademarksQuery.Where(t => t.Wordmark == searchTerm) :
                        trademarksQuery.Where(t => t.Wordmark.Contains(searchTerm));
                }
                else if (filter.SearchBy == TrademarkSearchBy.Owner)
                {
                    trademarksQuery = exact ?
                        trademarksQuery.Where(t => t.Owner == searchTerm) :
                        trademarksQuery.Where(t => t.Owner!=null && t.Owner.Contains(searchTerm));
                }

                else if (filter.SearchBy == TrademarkSearchBy.Number)
                {
                    trademarksQuery = exact ?
                        trademarksQuery.Where(t => t.SourceId == searchTerm || t.RegistrationNumber == searchTerm) :
                        trademarksQuery.Where(t => t.SourceId.Contains(searchTerm) || t.RegistrationNumber!.Contains(searchTerm));
                }
            }

            return trademarksQuery.AsNoTracking();
        }
    }
}
