using IPNoticeHub.Shared.Enums;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public sealed class TrademarkSearchFilter
    {
        public DataProvider? Provider { get; init; }

        public int[]? ClassNumbers { get; init; }

        public TrademarkStatusCategory? Status { get; init; }

        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;

        public string? SearchTerm { get; init; }

        public bool ExactMatch { get; init; }
    }
}