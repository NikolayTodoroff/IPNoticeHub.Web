using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.Models.Application
{
    public class TrademarkSearchResultsViewModel
    {
        public string? Query { get; set; }
        public TrademarkClass? Class { get; set; }
        public TrademarkStatusCategory? Status { get; set; }
        public TrademarkSearchBy? SearchBy { get; set; }
        public DataProvider? Office { get; set; }
        public SearchMode? Mode { get; set; }
        public IEnumerable<TmSearchResultSingleItemViewModel> Results { get; set; } = Enumerable.Empty<TmSearchResultSingleItemViewModel>();
        public int ResultsCount { get; set; }
    }
}
