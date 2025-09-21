namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkSearchResultDTO
    {
        public Guid Id { get; init; }
        public string RegistrationNumber { get; init; } = string.Empty;
        public string Wordmark { get; init; } = string.Empty;
        public string Owner { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
    }
}
