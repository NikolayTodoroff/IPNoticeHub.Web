namespace IPNoticeHub.Web.Models.PdfGeneration
{
    public interface IPdfLetterViewModel
    {
        Guid PublicId { get; }

        string WorkTitle { get; set; }
        string? RegistrationNumber { get; set; }

        string SenderName { get; set; }
        string SenderAddress { get; set; }
        string RecipientName { get; set; }
        string RecipientAddress { get; set; }

        string BodyTemplate { get; set; }


        // Optional – used only by CeaseDesist (partial will show it conditionally)
        string? AdditionalFacts { get; set; }
    }
}
