namespace IPNoticeHub.Web.Models.Trademarks
{
    public sealed class TrademarkCollectionViewModel
    {
        // Trademark registration that has been added to the user's collection.
        public IReadOnlyList<TrademarkSingleItemViewModel> Results { get; init; }
            = Array.Empty<TrademarkSingleItemViewModel>();


        // CurrentPage, PageSize and computed TotalPages/HasPreviousPage/HasNextPage providing paging information.
        public int Total { get; set; }


        public int CurrentPage { get; init; }


        public int PageSize { get; set; }


        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);


        public bool HasPreviousPage => CurrentPage > 1;


        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
