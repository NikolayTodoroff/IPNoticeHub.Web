using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.LegalDocumentConstants;
using static IPNoticeHub.Shared.Constants.ValidationConstants.InfringementPlaceholderConstants;

namespace IPNoticeHub.Application.DocumentLibrary.DTOs
{
    public class DocumentCreateDto
    {
        [Required]
        public Guid RelatedPublicId { get; set; }
        public DocumentSourceType SourceType { get; set; }
        public LetterTemplateType TemplateType { get; set; }

        [MaxLength(LegalDocumentsTitleMaxLength)]
        public string? DocumentTitle { get; set; }

        [MaxLength(IpTitleMaxLength)]
        public string? IpTitle { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        [Required, MaxLength(SenderNameMaxLength)]
        public string SenderName { get; set; } = string.Empty;

        [Required, MaxLength(SenderAddressMaxLength)]
        public string SenderAddress { get; set; } = string.Empty;

        [MaxLength(SenderEmailMaxLength)]
        [EmailAddress]
        public string? SenderEmail { get; set; }

        [Required, MaxLength(RecipientNameMaxLength)]
        public string RecipientName { get; set; } = string.Empty;

        [Required, MaxLength(RecipientAddressMaxLength)]
        public string RecipientAddress { get; set; } = string.Empty;

        [MaxLength(RecipientEmailMaxLength)]
        [EmailAddress]
        public string? RecipientEmail { get; set; }

        public DateTime LetterDate { get; set; }

        [MaxLength(InfringingUrlMaxLength)]
        public string? InfringingUrl { get; set; }

        [MaxLength(GoodFaithStatementMaxLength)]
        public string? GoodFaithStatement { get; set; }

        [MaxLength(AdditionalFactsMaxLength)]
        public string? AdditionalFacts { get; set; }

        public int? YearOfCreation { get; set; }

        public DateTime? DateOfPublication { get; set; }

        [MaxLength(NationOfFirstPublicationMaxLength)]
        public string? NationOfFirstPublication { get; set; }

        [Required]
        public string BodyTemplate { get; set; } = string.Empty;
    }
}
