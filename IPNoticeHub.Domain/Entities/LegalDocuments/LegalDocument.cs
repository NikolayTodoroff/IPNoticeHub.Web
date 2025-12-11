using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Shared.Constants.ValidationConstants.LegalDocumentConstants;
using static IPNoticeHub.Shared.Constants.ValidationConstants.InfringementPlaceholderConstants;

namespace IPNoticeHub.Domain.Entities.LegalDocuments
{
    public class LegalDocument
    {
        [Key]
        public int LegalDocumentId { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        [Required]
        public Guid RelatedPublicId { get; set; }

        public DocumentSourceType SourceType { get; set; }

        public LetterTemplateType TemplateType { get; set; }

        [Required, MaxLength(LegalDocumentsTitleMaxLength)]
        public string DocumentTitle { get; set; } = null!;

        [MaxLength(IpTitleMaxLength)]
        public string? IpTitle { get; set; }

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDeleted { get; set; }

        [Required, MaxLength(SenderNameMaxLength)]
        public string SenderName { get; set; } = null!;

        [Required, MaxLength(SenderAddressMaxLength)]
        public string SenderAddress { get; set; } = null!;

        [MaxLength(SenderEmailMaxLength)]
        public string? SenderEmail { get; set; }

        [Required, MaxLength(RecipientNameMaxLength)]
        public string RecipientName { get; set; } = null!;

        [Required, MaxLength(RecipientAddressMaxLength)]
        public string RecipientAddress { get; set; } = null!;

        [MaxLength(RecipientEmailMaxLength)]
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
        public string BodyTemplate { get; set; } = null!;
    }
}
