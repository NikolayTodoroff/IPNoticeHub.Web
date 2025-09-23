namespace IPNoticeHub.Data.Repositories.Application.Abstractions
{
    public interface ITrademarkStatusSnapshotRepository
    {
        Task<(int? StatusCodeRaw, string StatusDetail, DateTime? StatusDateUtc)?> GetStatusSnapshotAsync(int trademarkId, CancellationToken ct);
    }
}
