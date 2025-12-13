using IPNoticeHub.Domain.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;

namespace IPNoticeHub.Data.Repositories.Application.Implementations
{
    public sealed class TrademarkReadRepository : ITrademarkSearchRepository
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
    }
}
