namespace IPNoticeHub.Data.Repositories.TrademarkSearch.Abstractions
{
    public interface ITrademarkStatusSnapshotRepository
    {
        Task<(int? StatusCodeRaw, string StatusDetail, DateTime? StatusDateUtc)?> GetStatusSnapshotAsync(int trademarkId, CancellationToken ct);
    }
}
