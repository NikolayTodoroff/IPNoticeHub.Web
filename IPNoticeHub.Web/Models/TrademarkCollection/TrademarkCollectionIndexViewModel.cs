namespace IPNoticeHub.Web.Models.TrademarkCollection
{
    public sealed class TrademarkCollectionIndexViewModel
    {
        // Trademark registration that has been added to the user's collection.
        public IReadOnlyList<TrademarkCollectionItemViewModel> Results { get; init; }
            = Array.Empty<TrademarkCollectionItemViewModel>();


        // CurrentPage, PageSize and computed TotalPages/HasPreviousPage/HasNextPage providing paging information.
        public int Total { get; set; }


        public int CurrentPage { get; init; }


        public int PageSize { get; set; }


        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);


        public bool HasPreviousPage => CurrentPage > 1;


        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
