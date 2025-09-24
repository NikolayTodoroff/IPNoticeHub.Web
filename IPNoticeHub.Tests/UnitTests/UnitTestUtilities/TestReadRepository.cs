using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Tests.UnitTests.TestUtilities
{
    /// <summary>  
    /// A lightweight read-only repository designed for testing purposes, replicating the behavior of the actual repository.  
    /// It performs no-tracking queries and includes related classes for filtering.
    /// </summary>  
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
