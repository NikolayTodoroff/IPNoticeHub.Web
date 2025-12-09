using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions;
using Microsoft.EntityFrameworkCore;

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
