using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.StatusMessages;

namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public sealed class DmcaViewModel : IPdfLetterViewModel
    {
        public Guid PublicId { get; init; }

        public string WorkTitle { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; } = string.Empty;

        public int? YearOfCreation { get; set; }

        public DateTime? DateOfPublication { get; set; }

        public string? NationOfFirstPublication { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string SenderAddress { get; set; } = string.Empty;

        [EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientAddress { get; set; } = string.Empty;

        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        public string? InfringingUrl { get; set; }

        public string? AdditionalFacts { get; set; }

        public string GoodFaithStatement { get; set; } =
            DmcaDocumentTemplates.GoodFaithStatement;

        public string BodyTemplate { get; set; } =
            DmcaDocumentTemplates.BodyTemplate;
    }
}
