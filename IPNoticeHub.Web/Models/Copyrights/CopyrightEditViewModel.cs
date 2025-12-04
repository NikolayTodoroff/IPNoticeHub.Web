using IPNoticeHub.Common.EnumConstants;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.CopyrightRegistrationConstants;

namespace IPNoticeHub.Web.Models.Copyrights
{
    public sealed class CopyrightEditViewModel
    {
        public Guid PublicId { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string RegistrationNumber { get; init; } = string.Empty;

        public CopyrightWorkType WorkType { get; set; }

        [MaxLength(WorkTypeMaxLength)]
        public string? OtherWorkType { get; set; }

        [MaxLength(TitleMaxLength)]
        public string Title { get; init; } = string.Empty;

        public int? YearOfCreation { get; init; }

        public DateTime? DateOfPublication { get; init; }

        [MaxLength(OwnerNameMaxLength)]
        public string Owner { get; init; } = string.Empty;

        [MaxLength(NationOfFirstPublicationMaxLength)]
        public string? NationOfFirstPublication { get; init; }
    }
}
