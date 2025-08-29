namespace IPNoticeHub.Services.Copyrights.DTOs
{
    public sealed class CopyrightDetailsDTO
    {
        public Guid PublicId { get; init; }

        public string RegistrationNumber { get; init; } = string.Empty;

        public string TypeOfWork { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        public string Owner { get; init; } = string.Empty;

        public string? NationOfFirstPublication { get; init; }
    }
}
