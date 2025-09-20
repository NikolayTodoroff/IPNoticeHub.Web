namespace IPNoticeHub.Services.Trademarks.DTOs
{
    public sealed class TrademarkSearchResultDTO
    {
        public string RegistrationNumber { get; init; } = "";
        public string Mark { get; init; } = "";
        public string Owner { get; init; } = "";
        public string Status { get; init; } = "";
    }
}
