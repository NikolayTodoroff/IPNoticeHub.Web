using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Application.Implementations
{
    public class TrademarkReadRepository : ITrademarkReadRepository
    {
        private readonly IPNoticeHubDbContext dbContext;
        public TrademarkReadRepository(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<TrademarkEntity> TrademarkQueryNoTracking()
        {
            return dbContext.TrademarkRegistrations.AsNoTracking();
        }
    }
}
