namespace IPNoticeHub.Services.Copyrights.DTOs
{
    public sealed class CopyrightListItemDTO
    {
        public Guid PublicId { get; init; }

        public string RegistrationNumber { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Owner { get; init; } = string.Empty;

        public DateTime DateAdded { get; init; }
    }
}
