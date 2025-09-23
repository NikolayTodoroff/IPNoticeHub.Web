namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkWatchlistItemDTO
    {
        public int Id { get; init; }
        public string RegistrationNumber { get; init; } = "";
        public string Wordmark { get; init; } = "";
        public string Owner { get; init; } = "";
        public DateTime? AddedOnDate { get; init; }
        public string? InitialStatus { get; init; }
        public string CurrentStatus { get; init; } = "";
        public bool HasStatusChange => !string.IsNullOrEmpty(InitialStatus) && !string.Equals(InitialStatus, CurrentStatus, StringComparison.OrdinalIgnoreCase);
        public bool NotificationsEnabled { get; init; }
    }
}
