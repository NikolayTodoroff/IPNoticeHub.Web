namespace IPNoticeHub.Application.DTOs.DraftStoreDTOs
{
    public class DmcaDraftDto
    {
        public string? WorkTitle { get; init; }
        public string? RegistrationNumber { get; init; }

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        public string? NationOfFirstPublication { get; init; }

        public string? SenderName { get; init; }

        public string? SenderAddress { get; init; }

        public string? SenderEmail { get; init; }

        public string? RecipientName { get; init; }

        public string? RecipientAddress { get; init; }

        public string? RecipientEmail { get; init; }

        public string? InfringingUrl { get; init; }

        public string? AdditionalFacts { get; init; }
    }
}
