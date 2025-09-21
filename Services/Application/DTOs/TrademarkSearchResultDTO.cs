namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkSearchResultDTO
    {
        public string RegistrationNumber { get; init; } = "";
        public string Wordmark { get; init; } = "";
        public string Owner { get; init; } = "";
        public string Status { get; init; } = "";
    }
}
