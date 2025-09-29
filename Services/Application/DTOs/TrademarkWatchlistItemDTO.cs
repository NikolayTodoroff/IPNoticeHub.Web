namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkWatchlistItemDTO
    {
        public int Id { get; init; }
        public Guid PublicId { get; init; }
        public string RegistrationNumber { get; init; } = "";
        public string Wordmark { get; init; } = "";
        public string Owner { get; init; } = "";
        public DateTime? AddedOnDate { get; init; }
        public string? InitialStatus { get; init; }
        public string CurrentStatus { get; init; } = "";
        public bool HasStatusChange { get; init; }
        public bool NotificationsEnabled { get; init; }
    }
}
