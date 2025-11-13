namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public interface IPdfLetterViewModel
    {
        Guid PublicId { get; }

        string WorkTitle { get; set; }
        string? RegistrationNumber { get; set; }

        string SenderName { get; set; }
        string SenderAddress { get; set; }
        string SenderEmail { get; set; }
        string RecipientName { get; set; }
        string RecipientAddress { get; set; }
        string RecipientEmail { get; set; }
        string InfringingUrl { get; set; }

        string BodyTemplate { get; set; }


        // Optional – used only by Cease & Desist
        string? AdditionalFacts { get; set; }
    }
}
