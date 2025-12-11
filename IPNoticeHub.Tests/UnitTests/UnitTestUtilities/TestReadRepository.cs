using IPNoticeHub.Data;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{ 
    public class TestReadRepository : ITrademarkReadRepository
    {
        private readonly IPNoticeHubDbContext testDbContext;

        public TestReadRepository(IPNoticeHubDbContext testDbContext)
        {
            this.testDbContext = testDbContext;
        }

        public IQueryable<TrademarkEntity> TrademarkQueryNoTracking()
        {
            return testDbContext.TrademarkRegistrations.
                AsNoTracking().
                Include(t=>t.Classes).
                AsQueryable();
        }
    }
}
