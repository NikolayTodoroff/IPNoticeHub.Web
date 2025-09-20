using IPNoticeHub.Common.EnumConstants;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
namespace IPNoticeHub.Web.Models.Trademarks
{
    public class TrademarkSearchResultsViewModel
    {
        public string? Query { get; init; }
        public TrademarkClass? Class { get; init; }
        public TrademarkStatusCategory? Status { get; init; }
        public TrademarkSearchBy? SearchBy { get; init; }
        public DataProvider? Office { get; init; }
        public SearchMode? Mode { get; init; }
        public int Page { get; init; } = DefaultPage;
        public int PageSize { get; init; } = DefaultPageSize;

        public class TmSearchResultSingleItemViewModel
        {
            public string RegistrationNumber { get; set; } = "";
            public string Mark { get; set; } = "";
            public string Owner { get; set; } = "";
            public string Status { get; set; } = "";
        }
    }
}
