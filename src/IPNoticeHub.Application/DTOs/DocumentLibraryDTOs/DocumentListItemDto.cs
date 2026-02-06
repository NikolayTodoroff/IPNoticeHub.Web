using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.LegalDocumentConstants;

namespace IPNoticeHub.Application.DTOs.DocumentLibraryDTOs
{
    public class DocumentListItemDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(LegalDocumentsTitleMaxLength)]
        public string DocumentTitle { get; set; } = string.Empty;

        public DocumentSourceType SourceType { get; set; }

        public LetterTemplateType TemplateType { get; set; }

        [MaxLength(IpTitleMaxLength)]
        public string? IpTitle { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
