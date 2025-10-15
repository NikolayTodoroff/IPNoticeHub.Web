using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.ViewModels.Trademarks
{
    public sealed class TrademarkDetailsViewModel
    {
        public int? Id { get; init; }
        public Guid PublicId { get; init; }
        public string? Wordmark { get; init; }
        public string? Owner { get; init; }
        public string? SourceId { get; init; }
        public string? RegistrationNumber { get; init; }
        public TrademarkStatusCategory Status { get; init; }
        public string? GoodsAndServices { get; init; }
        public DateTime? FilingDate { get; init; }
        public DateTime? RegistrationDate { get; init; }
        public string? MarkImageUrl { get; init; }
        public DataProvider Provider { get; init; }
        public IReadOnlyList<int> Classes { get; init; } = Array.Empty<int>();

        public bool IsAuthenticated { get; set; }
        public bool IsInCollection { get; init; }
        public bool IsInWatchlist { get; init; }
    }
}
