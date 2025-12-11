using IPNoticeHub.Domain.Entities.Trademarks;
using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Data;

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
    }
}
