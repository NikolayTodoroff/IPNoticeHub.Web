using IPNoticeHub.Common.EnumConstants;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.LegalDocumentConstants;

namespace IPNoticeHub.Services.DocumentLibrary.DTOs
{
    public class DocumentCreateDto
    {
        public Guid RelatedPublicId { get; set; }

        public DocumentSourceType SourceType { get; set; }

        public LetterTemplateType TemplateType { get; set; }

        [Required]
        [MaxLength(LegalDocumentsTitleMaxLength)]
        public string? DocumentTitle { get; set; }

        [MaxLength(IpTitleMaxLength)]
        public string? IpTitle { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        [Required]
        public string BodyTemplate { get; set; } = string.Empty;
    }
}
