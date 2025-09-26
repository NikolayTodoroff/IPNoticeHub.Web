using IPNoticeHub.Data.Repositories.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.Application.Implementations
{
    public class TrademarkStatusSnapshotRepository : ITrademarkStatusSnapshotRepository
    {
        private readonly IPNoticeHubDbContext dbContext;
        public TrademarkStatusSnapshotRepository(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<(int? StatusCodeRaw, string StatusDetail, DateTime? StatusDateUtc)?> GetStatusSnapshotAsync(int trademarkId, CancellationToken cancellationToken)
        {
            var snapshotResult = await dbContext.TrademarkRegistrations
                .AsNoTracking()
                .Where(t => t.Id == trademarkId)
                .Select(t => new
                {
                    t.StatusCodeRaw,
                    t.StatusDetail,
                    t.StatusDateUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (snapshotResult == null)
            {
                return null;
            }

            return (snapshotResult.StatusCodeRaw, snapshotResult.StatusDetail ?? string.Empty, snapshotResult.StatusDateUtc);
        }
    }
}



