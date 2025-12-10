using IPNoticeHub.Shared.Enums;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Services.TrademarkSearch.DTOs
{
    public sealed class TrademarkSearchQueryDto
    {
        public string? Query { get; init; }

        public TrademarkClass? Class { get; init; }

        public TrademarkStatusCategory? Status { get; init; }

        public TrademarkSearchBy? SearchBy { get; init; }

        public DataProvider? Office { get; init; }

        public SearchMode? Mode { get; init; }

        public int Page { get; init; } = DefaultPage;

        public int PageSize { get; init; } = DefaultPageSize;
    }
}
