using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Support;
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

        public async Task<PagedResult<TrademarkSearchResultDto>> SearchAsync(
            TrademarkSearchQueryDto searchQuery, 
            CancellationToken cancellationToken = default)
        {
            var realRepo = new TrademarkReadRepository(testDbContext);
            return await realRepo.SearchAsync(searchQuery, cancellationToken);
        }

        public IQueryable<TrademarkEntity> TrademarkQueryNoTracking()
        {
            return testDbContext.TrademarkRegistrations.
                Include(t => t.Classes).
                AsNoTracking();
        }
    }
}
