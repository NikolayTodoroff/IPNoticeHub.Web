using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public sealed class CeaseDesistViewModel : IPdfLetterViewModel
    {
        public Guid PublicId { get; init; }

        public string WorkTitle { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; }

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

        public string BodyTemplate { get; set; } = "To {{RecipientName}}," +
            "\n\nThis letter demands that you cease and desist from unauthorized use of " +
            "\"{{WorkTitle}}\" (Reg. No. {{RegistrationNumber}})." +
            "\n\n{{AdditionalFacts}}\n\nSincerely,\n{{SenderName}}\n{{SenderAddress}}";
    }
}
