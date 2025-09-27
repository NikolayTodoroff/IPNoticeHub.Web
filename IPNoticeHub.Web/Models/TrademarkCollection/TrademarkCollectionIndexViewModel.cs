namespace IPNoticeHub.Web.Models.TrademarkCollection
{
    public sealed class TrademarkCollectionIndexViewModel
    {
        public IReadOnlyList<TrademarkCollectionItemViewModel> Results { get; init; }
            = Array.Empty<TrademarkCollectionItemViewModel>();

        public int CurrentPage { get; init; }
        public int ResultsPerPage { get; init; }
        public int ResultsCount { get; init; }
    }
}
