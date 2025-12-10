using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.CopyrightRegistrationConstants;

namespace IPNoticeHub.Services.Copyrights.DTOs
{
    public class CopyrightCreateDto
    {
        [Required, MaxLength(RegistrationNumberMaxLength)]
        public string RegistrationNumber { get; init; } = string.Empty;

        public CopyrightWorkType WorkType { get; init; }

        public string? OtherWorkType { get; init; }

        [Required, MaxLength(TitleMaxLength)]
        public string Title { get; init; } = string.Empty;

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        [Required, MaxLength(OwnerNameMaxLength)]
        public string Owner { get; init; } = string.Empty;

        [MaxLength(NationOfFirstPublicationMaxLength)]
        public string? NationOfFirstPublication { get; init; }
    }
}
