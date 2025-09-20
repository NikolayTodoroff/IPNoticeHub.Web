using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.Models.Trademarks
{
    public class TrademarkSearchResultsViewModel
    {
        public string? Query { get; set; }
        public TrademarkClass? Class { get; set; }
        public TrademarkStatusCategory? Status { get; set; }
        public TrademarkSearchBy? SearchBy { get; set; }
        public DataProvider? Office { get; set; }
        public SearchMode? Mode { get; set; }
        public IEnumerable<TrademarkResultRowViewModel> Results { get; set; } = 
            Enumerable.Empty<TrademarkResultRowViewModel>();
        public int Total { get; set; }
    }

    public class TrademarkResultRowViewModel
    {
        public string RegistrationNumber { get; set; } = "";
        public string Mark { get; set; } = "";
        public string Owner { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
