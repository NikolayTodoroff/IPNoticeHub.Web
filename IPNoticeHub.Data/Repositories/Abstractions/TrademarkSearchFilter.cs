using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Data.Repositories.Abstractions
{
    public sealed class TrademarkSearchFilter
    {
        // The data provider to search within. Maps to UI "Country" (e.g., USPTO=US, EUIPO=EU, WIPO=Intl).
        public DataProvider? Provider { get; init; }


        // The list of trademark class numbers to filter by. Valid values range from 1 to 45.
        public int[]? ClassNumbers { get; init; }


        // The status category of the trademark to filter by (e.g., Pending, Registered, Cancelled, Abandoned).
        public TrademarkStatusCategory? Status { get; init; }


        // Specifies the type of search to perform (e.g., by Wordmark, Owner, or Registration Number).
        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;


        // The search query string. Represents the wordmark, owner name, or registration number to search for.
        // Renamed property from 'Query' to 'SearchTerm' for better clarity.
        public string? SearchTerm { get; init; }


        // Indicates whether the search should match the query exactly (true) or perform a "contains" search (false).
        public bool ExactMatch { get; init; }
    }
}