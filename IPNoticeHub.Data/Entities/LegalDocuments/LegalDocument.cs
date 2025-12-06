using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Common.ValidationConstants.LegalDocumentConstants;

namespace IPNoticeHub.Data.Entities.LegalDocuments
{
    public class LegalDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        [Required]
        public Guid RelatedPublicId { get; set; }

        [Required]
        [MaxLength(LegalDocumentsTitleMaxLength)]
        public string DocumentTitle { get; set; } = null!;

        [Required]
        public DocumentSourceType SourceType { get; set; }

        [Required]
        public LetterTemplateType TemplateType { get; set; }

        [MaxLength(IpTitleMaxLength)]
        public string? IpTitle { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        [Required]
        public string BodyTemplate { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
