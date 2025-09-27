using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Web.Models.TrademarkCollection
{
    public sealed class TrademarkCollectionItemViewModel
    {
        public int Id { get; init; }
        public Guid PublicId { get; init; }
        public string Wordmark { get; init; } = null!;
        public string SourceId { get; init; } = null!;
        public string? Owner { get; init; }
        public TrademarkStatusCategory Status { get; init; }
        public int[] Classes { get; init; } = Array.Empty<int>();
        public DataProvider Provider { get; init; }
    }
}
