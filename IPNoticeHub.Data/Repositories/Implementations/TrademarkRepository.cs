using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Implementations
{
    public class TrademarkRepository : ITrademarkRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public TrademarkRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public Task<bool> ExistsAsync(int id)
        {
            return dbContext.TrademarkRegistrations.AnyAsync(t => t.Id == id);
        }

        public async Task<TrademarkEntity?> GetByPublicIdAsync(Guid publicId, bool asNoTracking = true)
        {
             return await dbContext.TrademarkRegistrations.
                Include(t => t.Classes).
                Include(t => t.Events).
                AsSplitQuery().
                AsNoTrackingIf(asNoTracking == true).
                SingleOrDefaultAsync(t => t.PublicId == publicId);
        }

        public Task<int?> GetIdByPublicIdAsync(Guid publicId)
        {
            return dbContext.TrademarkRegistrations.
                Where(t=>t.PublicId == publicId).
                Select(t=>(int?)t.Id).
                FirstOrDefaultAsync();
        }

        // Queries the TrademarkEntity table based on the provided filter and optional navigation properties.
        public IQueryable<TrademarkEntity> Query(TrademarkSearchFilter filter, bool includeNavProp = false)
        {
            IQueryable<TrademarkEntity> trademarksQuery = dbContext.TrademarkRegistrations;

            if (includeNavProp)
            {
                trademarksQuery = trademarksQuery.Include(t => t.Classes).
                Include(t => t.Events);
            }

            if (filter.Provider.HasValue)
            {
                trademarksQuery = trademarksQuery.Where(t => t.Source == filter.Provider);
            }

            if (filter.Status.HasValue)
            {
                trademarksQuery = trademarksQuery.Where(t => t.StatusCategory == filter.Status.Value);
            }

            if (filter.ClassNumbers != null && filter.ClassNumbers.Length > 0)
            {
                trademarksQuery = trademarksQuery.Where(t => t.Classes
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
                        trademarksQuery.Where(t => t.Owner!.Contains(searchTerm));
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
