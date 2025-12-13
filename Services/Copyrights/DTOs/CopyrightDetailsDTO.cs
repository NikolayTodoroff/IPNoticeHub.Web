using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.CopyrightRegistrationConstants;

namespace IPNoticeHub.Application.Copyrights.DTOs
{
    public sealed class CopyrightDetailsDto
    {
        public Guid PublicId { get; init; }

        [Required, MaxLength(RegistrationNumberMaxLength)]
        public string RegistrationNumber { get; init; } = string.Empty;

        [Required, MaxLength(WorkTypeMaxLength)]
        public string TypeOfWork { get; init; } = string.Empty;

        [Required, MaxLength(TitleMaxLength)]
        public string Title { get; init; } = string.Empty;

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        [Required, MaxLength(OwnerNameMaxLength)]
        public string Owner { get; init; } = string.Empty;

        public string? NationOfFirstPublication { get; init; }
    }
}
