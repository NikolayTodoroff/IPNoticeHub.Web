using IPNoticeHub.Data.Entities.ApplicationUser;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.EntityValidationConstants.CopyrightRegistrationConstants;

namespace IPNoticeHub.Data.Entities.CopyrightRegistration
{
    public class CopyrightEntity
    {
        [Key]
        [Comment("Primary key for the Copyright entity")]
        public int Id { get; set; }

        [Comment("Unique identifier for the Copyright, generated automatically")]
        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(RegistrationNumberMaxLength)]
        [Comment("Official copyright registration number, e.g. VA0002288838")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required, MaxLength(WorkTypeMaxLength)]
        [Comment("Type of work, e.g. Literary Work, Visual Material, Music, Software")]
        public string TypeOfWork { get; set; } = string.Empty;

        [Required, MaxLength(TitleMaxLength)]
        [Comment("Title of the work being registered")]
        public string Title { get; set; } = string.Empty;

        [Comment("Year of creation of the work (if provided)")]
        public int? YearOfCreation { get; set; }

        [Comment("Date of publication, if the work has been published")]
        public DateTime? DateOfPublication { get; set; }

        [Required, MaxLength(OwnerNameMaxLength)]
        [Comment("Copyright claimant, usually the author or company")]
        public string Owner { get; set; } = string.Empty;

        [MaxLength(NationOfFirstPublicationMaxLength)]
        [Comment("Nation of first publication")]
        public string? NationOfFirstPublication { get; set; }


        [Comment("Navigation property for the many - to - many relationship between CopyrightRegistration and UserCopyrights")]
        public ICollection<UserCopyright> UserCopyrights { get; set; } = new List<UserCopyright>();
    }
}
