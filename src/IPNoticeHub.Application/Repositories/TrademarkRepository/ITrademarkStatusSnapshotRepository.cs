namespace IPNoticeHub.Application.Repositories.TrademarkRepository
{
    public interface ITrademarkStatusSnapshotRepository
    {
        Task<(int? StatusCodeRaw, string StatusDetail, DateTime? StatusDateUtc)?> 
            GetStatusSnapshotAsync(int trademarkId, CancellationToken ct);
    }
}
