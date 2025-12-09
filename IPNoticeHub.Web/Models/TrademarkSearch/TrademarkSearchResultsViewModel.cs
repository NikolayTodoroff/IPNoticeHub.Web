using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.Models.TrademarkSearch
{
    public class TrademarkSearchResultsViewModel
    {
        public string? Query { get; set; }

        public TrademarkClass? Class { get; set; }

        public TrademarkStatusCategory? Status { get; set; }

        public TrademarkSearchBy? SearchBy { get; set; }

        public DataProvider? Office { get; set; }

        public SearchMode? Mode { get; set; }

        public IEnumerable<TreademarkSearchResultSingleItemViewModel> 
            Results { get; set; } = Enumerable.Empty<
                TreademarkSearchResultSingleItemViewModel>();


        public int Total { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : 
            (int)Math.Ceiling(Total / (double)PageSize);

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
