namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkSearchResultDTO
    {
        public int Id { get; init; }
        public Guid PublicId { get; init; }
        public string RegistrationNumber { get; init; } = string.Empty;
        public string Wordmark { get; init; } = string.Empty;
        public string Owner { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime? RegistrationDate { get; init; }
        
    }
}
