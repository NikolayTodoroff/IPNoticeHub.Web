using IPNoticeHub.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.CopyrightRegistrationConstants;

namespace IPNoticeHub.Domain.Entities.Copyrights
{
    public class CopyrightEntity
    {
        [Key]
        public int Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(RegistrationNumberMaxLength)]
        public string RegistrationNumber { get; set; } = null!;

        [Required, MaxLength(WorkTypeMaxLength)]
        public string TypeOfWork { get; set; } = null!;

        [Required, MaxLength(TitleMaxLength)]
        public string Title { get; set; } = null!;

        public int? YearOfCreation { get; set; }

        public DateTime? DateOfPublication { get; set; }

        [Required, MaxLength(OwnerNameMaxLength)]
        public string Owner { get; set; } = null!;

        [MaxLength(NationOfFirstPublicationMaxLength)]
        public string? NationOfFirstPublication { get; set; }

        public ICollection<UserCopyright> UserCopyrights { get; set; } = new List<UserCopyright>();
    }
}
