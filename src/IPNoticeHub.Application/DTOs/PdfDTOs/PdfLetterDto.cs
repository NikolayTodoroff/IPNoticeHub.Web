namespace IPNoticeHub.Application.DTOs.PdfDTOs
{
    public sealed record PdfLetterDto
    {
        public string DocumentType { get; init; } = string.Empty;

        public string DocumentTitle { get; init; } = string.Empty;

        public DateTime DateUtc { get; init; } = DateTime.UtcNow;

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

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        public string? NationOfFirstPublication { get; init; }

        public string? GoodFaithStatement { get; init; }

        public string BodyTemplate { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, string> Tokens { get; init; }
        = new Dictionary<string, string>();
    }
}
