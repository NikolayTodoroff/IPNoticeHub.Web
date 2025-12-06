using IPNoticeHub.Common.EnumConstants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IPNoticeHub.Data.Entities.Identity;
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
        public DocumentSourceType SourceType { get; set; }

        [Required]
        public LetterTemplateType TemplateType { get; set; }

        [Required]
        [MaxLength(LegalDocumentsTitleMaxLength)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string BodyTemplate { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
