using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Services.DTOs.Trademarks
{
    public sealed class TrademarkDetailsDTO
    {
        public Guid PublicId { get; init; }

        public string Wordmark { get; init; } = null!;

        public string? Owner { get; init; }

        public string SourceId { get; init; } = null!;

        public string? RegistrationNumber { get; init; }

        public TrademarkStatusCategory Status { get; init; }

        public DateTime? FilingDate { get; init; }

        public DateTime? RegistrationDate { get; init; }

        public string? MarkImageUrl { get; init; }

        public DataProvider Provider { get; init; }

        public IReadOnlyList<int> Classes { get; init; } = new List<int>();

        public IReadOnlyList<(DateTime Date, string Code, string? Description)> Events { get; init; }
            = new List<(DateTime, string, string?)>();
    }
}
