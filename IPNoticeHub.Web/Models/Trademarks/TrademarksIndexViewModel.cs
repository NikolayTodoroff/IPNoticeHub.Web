using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.Models.Trademarks
{
    /// <summary>
    /// ViewModel for the Trademarks Index page (search results).
    /// Combines the current filter, paging info, and the list of results.
    /// </summary>
    public sealed class TrademarksIndexViewModel
    {
        public string? SearchTerm { get; init; }
        public string SearchBy { get; init; } = "Wordmark"; // Wordmark | Owner | Number
        public DataProvider? Provider { get; init; }
        public TrademarkStatusCategory? Status { get; init; }
        public IReadOnlyList<int> ClassNumbers { get; init; } = Array.Empty<int>();
        public bool ExactMatch { get; init; }


        public int CurrentPage { get; init; }
        public int ResultsPerPage { get; init; }
        public int ResultsCount { get; init; }


        public IReadOnlyList<TrademarkListItemViewModel> Results { get; init; } = Array.Empty<TrademarkListItemViewModel>();
    }
}
