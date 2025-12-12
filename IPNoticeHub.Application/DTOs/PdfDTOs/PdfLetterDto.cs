namespace IPNoticeHub.Application.DTOs.PdfDTOs
{
    public sealed class PdfLetterDto
    {
        public string DocumentType { get; init; } = string.Empty;
        public string DocumentTitle { get; init; } = string.Empty;

        public string WorkTitle { get; init; } = string.Empty;
        public string? RegistrationNumber { get; init; } = string.Empty;

        public string SenderName { get; init; } = string.Empty;
        public string? SenderAddress { get; init; } = string.Empty;
        public string? SenderEmail { get; init; } = string.Empty;

        public string RecipientName { get; init; } = string.Empty;
        public string? RecipientAddress { get; init; } = string.Empty;
        public string? RecipientEmail { get; init; } = string.Empty;

        public string? InfringingUrl { get; init; }
        public string? AdditionalFacts { get; init; }

        public string BodyTemplate { get; init; } = string.Empty;
        public DateTime DateUtc { get; init; } = DateTime.UtcNow;

        public IReadOnlyDictionary<string, string> Tokens { get; init; }
        = new Dictionary<string, string>();
    }
}
