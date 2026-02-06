using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Web.Models.PdfGeneration;

namespace IPNoticeHub.Web.WebHelpers.Mappings
{
    public class LegalDocumentMapping
    {
        public static CeaseDesistViewModel MapDocumentToCeaseDesistViewModel(LegalDocument document)
        {
            return new CeaseDesistViewModel()
            {
               PublicId = document.RelatedPublicId,
                WorkTitle = document.IpTitle ?? "Intellectual property identified by registration",
                RegistrationNumber = document.RegistrationNumber,
                SenderName = document.SenderName,
                SenderAddress = document.SenderAddress,
                SenderEmail = document.SenderEmail!,
                RecipientName = document.RecipientName,
                RecipientAddress = document.RecipientAddress,
                RecipientEmail = document.RecipientEmail!,
                InfringingUrl = document.InfringingUrl!,
                AdditionalFacts = document.AdditionalFacts,
                BodyTemplate = document.BodyTemplate
            };
        }

        public static DmcaViewModel MapDocumentToDmcaViewModel(LegalDocument document)
        {
            return new DmcaViewModel()
            {
                PublicId = document.RelatedPublicId,
                WorkTitle = document.IpTitle ?? "Intellectual property identified by registration",
                RegistrationNumber = document.RegistrationNumber,
                YearOfCreation = document.YearOfCreation,
                DateOfPublication = document.DateOfPublication,
                NationOfFirstPublication = document.NationOfFirstPublication,
                SenderName = document.SenderName, 
                SenderAddress = document.SenderAddress,
                SenderEmail= document.SenderEmail!,
                RecipientName= document.RecipientName,
                RecipientAddress= document.RecipientAddress,
                RecipientEmail= document.RecipientEmail!,
                InfringingUrl = document.InfringingUrl!,
                AdditionalFacts= document.AdditionalFacts,
                GoodFaithStatement = document.GoodFaithStatement!,
                BodyTemplate = document.BodyTemplate
            };
        }
    }
}
