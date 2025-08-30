using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Services.Trademarks.DTOs
{
    public sealed class TrademarkListItemDTO
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
