using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public sealed class TrademarkSearchFilter
    {
        /// <summary>
        /// The data provider to search within. Maps to UI "Country" (e.g., USPTO=US, EUIPO=EU, WIPO=Intl).
        /// </summary>
        public DataProvider? Provider { get; init; }

        /// <summary>
        /// The list of trademark class numbers to filter by. Valid values range from 1 to 45.
        /// </summary>
        public int[]? ClassNumbers { get; init; }

        /// <summary>
        /// The status category of the trademark to filter by (e.g., Pending, Registered, Cancelled, Abandoned).
        /// </summary>
        public TrademarkStatusCategory? Status { get; init; }

        /// <summary>
        /// Specifies the type of search to perform (e.g., by Wordmark, Owner, or Registration Number).
        /// </summary>
        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;

        /// <summary>
        /// The search query string. Represents the wordmark, owner name, or registration number to search for.
        /// Renamed property from 'Query' to 'SearchTerm' for better clarity.
        /// </summary>
        public string? SearchTerm { get; init; }

        /// <summary>
        /// Indicates whether the search should match the query exactly (true) or perform a "contains" search (false).
        /// </summary>
        public bool ExactMatch { get; init; }
    }
}