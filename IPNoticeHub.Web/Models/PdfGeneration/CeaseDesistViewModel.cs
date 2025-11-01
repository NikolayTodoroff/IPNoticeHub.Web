using System.ComponentModel.DataAnnotations;


namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public sealed class CeaseDesistViewModel
    {
        // Record identity (Trademark or Copyright Public Id)
        public Guid PublicId { get; init; }


        // Autopopulated fields
        [Required]
        public string WorkTitle { get; init; } = string.Empty;

        public string? RegistrationNumber { get; init; }


        // Sender and Recipient

        [Required]
        public string SenderName { get; set; } = string.Empty;

        public string SenderAddress { get; set; } = string.Empty;

        [Required] 
        public string RecipientName { get; set; } = string.Empty;

        public string RecipientAddress { get; set; } = string.Empty;


        // Optional narrative
        public string? AdditionalFacts { get; set; }


        // Editable template
        [Required]
        public string BodyTemplate { get; set; } = "To {{RecipientName}}," +
            "\n\nThis letter demands that you cease and desist from unauthorized use of " +
            "\"{{WorkTitle}}\" (Reg. No. {{RegistrationNumber}})." +
            "\n\n{{AdditionalFacts}}\n\nSincerely,\n{{SenderName}}\n{{SenderAddress}}";
    }
}
