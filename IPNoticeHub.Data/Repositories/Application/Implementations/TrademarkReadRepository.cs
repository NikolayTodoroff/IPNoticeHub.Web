using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Application.Implementations
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
    }
}
