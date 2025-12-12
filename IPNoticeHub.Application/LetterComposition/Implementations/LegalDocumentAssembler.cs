using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IPNoticeHub.Application.LetterComposition.Implementations
{
    public class LegalDocumentAssembler : ILegalDocumentAssembler
    {
        private readonly ILetterTemplateProvider templateProvider;

        public LegalDocumentAssembler(ILetterTemplateProvider templateProvider)
        {
            this.templateProvider = templateProvider;
        }

        public PdfLetterDto RebuildDocumentSnapshot(LegalDocument document)
        {
            var templateKey = LetterTemplateKeys.GenerateLetterKey(
                document.SourceType,
                document.TemplateType);

            var letterTemplate = templateProvider.GetTemplateByKey(templateKey);

            var letterBody = 
                letterTemplate?.BodyTemplate ?? 
                document.BodyTemplate ?? 
                string.Empty;

            return new PdfLetterDto
            {
                DocumentType = EnumDisplay.GetDisplayName(document.TemplateType),
                DocumentTitle = document.DocumentTitle,

                WorkTitle = document.IpTitle ?? "Intellectual Property",
                RegistrationNumber = document.RegistrationNumber,

                SenderName = document.SenderName,
                SenderEmail = document.SenderEmail,
                SenderAddress = document.SenderAddress,

                RecipientName = document.RecipientName,
                RecipientEmail = document.RecipientEmail,
                RecipientAddress = document.RecipientAddress,


                InfringingUrl = document.InfringingUrl,
                AdditionalFacts = document.AdditionalFacts,

                BodyTemplate = letterBody,
                DateUtc = DateTime.UtcNow,
                Tokens = new Dictionary<string, string>()
            };
        }

        private static class LetterTemplateKeys
        {
            public static string GenerateLetterKey(
                DocumentSourceType letterSource, 
                LetterTemplateType templateType)
            {
                return $"{letterSource}:{templateType}";
            }
        }

        private static class EnumDisplay
        {
            public static string GetDisplayName(Enum value)
            {
                var member = value.GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault();

                var display = member?
                    .GetCustomAttribute<DisplayAttribute>();

                return display?.Name ?? value.ToString();
            }
        }
    }
}
