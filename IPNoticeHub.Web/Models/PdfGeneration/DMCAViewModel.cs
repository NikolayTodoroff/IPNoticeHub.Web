using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public sealed class DMCAViewModel : IPdfLetterViewModel
    {
        // Record identity (Copyright Public Id)
        public Guid PublicId { get; init; }

        // Autopopulated fields
        [Required]
        [BindNever]
        public string WorkTitle { get; set; } = string.Empty;

        [Required]
        [BindNever]
        public string? RegistrationNumber { get; set; } = string.Empty;

        public int? YearOfCreation { get; set; }

        public DateTime? DateOfPublication { get; set; }

        public string? NationOfFirstPublication { get; set; }


        // Sender and Recipient
        [Required]
        public string SenderName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;

        public string SenderAddress { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;

        [EmailAddress]
        public string? RecipientEmail { get; set; }

        public string RecipientAddress { get; set; } = string.Empty;

        [Required, Url]
        public string InfringingUrl { get; set; } = string.Empty;

        // Optional narrative
        public string? AdditionalFacts { get; set; }

        // Statements
        [Required]
        public string GoodFaithStatement { get; set; } = "I have a good faith belief that the " +
            "disputed use of the copyrighted material is not authorized by the copyright owner, " +
            "its agent, or the law.";

        // Editable Template
        public string BodyTemplate { get; set; } = "Dear {{RecipientName}},\n\nI, {{SenderName}} ({{SenderEmail}}), " +
            "submit this DMCA notice concerning the work \"{{WorkTitle}}\" (Reg. No. {{RegistrationNumber}}). " +
            "The infringing material appears at {{InfringingUrl}}." +
            "\n\n{{GoodFaithStatement}}\n\nSincerely,\n{{SenderName}}\n{{SenderAddress}}";
    }
}
