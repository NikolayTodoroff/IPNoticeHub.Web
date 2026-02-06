using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.Abstractions;
using IPNoticeHub.Application.LetterComposition.TokenBuilder;
using IPNoticeHub.Application.Templates.Abstractions;

namespace IPNoticeHub.Application.LetterComposition.Implementations
{
    public class LetterAssembler : ILetterAssembler
    {
        private readonly ILetterTemplateProvider templateProvider;

        public LetterAssembler(ILetterTemplateProvider templateProvider)
        {
            this.templateProvider = templateProvider;
        }

        public PdfLetterDto RebuildLetterInput(LetterInputDto input)
        {
            var letterBody = input.BodyTemplate ?? 
                templateProvider.GetTemplateByKey(input.DocumentType)?.BodyTemplate ?? string.Empty;

            var dto = new PdfLetterDto
            {
                DocumentType = input.DocumentType,
                DocumentTitle = input.DocumentTitle,

                WorkTitle = input.WorkTitle,
                RegistrationNumber = input.RegistrationNumber,

                SenderName = input.SenderName,
                SenderEmail = input.SenderEmail,
                SenderAddress = input.SenderAddress,

                RecipientName = input.RecipientName,
                RecipientEmail = input.RecipientEmail,
                RecipientAddress = input.RecipientAddress,

                InfringingUrl = input.InfringingUrl,
                AdditionalFacts = input.AdditionalFacts,

                BodyTemplate = letterBody,
                DateUtc = input.LetterDateUtc ?? DateTime.UtcNow,

                YearOfCreation = input.YearOfCreation,
                DateOfPublication = input.DateOfPublication,
                NationOfFirstPublication = input.NationOfFirstPublication,
                GoodFaithStatement = input.GoodFaithStatement,
            };

            var tokens = 
                LetterTokenBuilder.BuildTokens(dto);

            return dto with { Tokens = tokens };
        }
    }
}

