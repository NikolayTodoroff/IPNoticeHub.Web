namespace IPNoticeHub.Application.DTOs.PdfDTOs
{
    public sealed class LetterTemplateDto
    {
        public string LetterKey { get; init; } = string.Empty;

        public string BodyTemplate { get; init; } = string.Empty;

        public string? SubjectTemplate { get; init; }
    }
}
