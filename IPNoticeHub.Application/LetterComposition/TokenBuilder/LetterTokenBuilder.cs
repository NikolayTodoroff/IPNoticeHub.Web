using IPNoticeHub.Application.DTOs.PdfDTOs;
using static IPNoticeHub.Shared.Constants.TokenConstants;
using static IPNoticeHub.Shared.Constants.DateTimeFormats.DefaultDateTimeFormat;

namespace IPNoticeHub.Application.LetterComposition.TokenBuilder
{
    public class LetterTokenBuilder
    {
        public static Dictionary<string, string> BuildTokens(PdfLetterDto dto)
        {
            var tokens = 
                new Dictionary<string, string>(StringComparer.Ordinal);

            void Add(string key, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value)) tokens[key] = value;    
            }

            Add(WorkTitle, dto.WorkTitle);
            Add(RegistrationNumber, dto.RegistrationNumber);
            Add(Date, dto.DateUtc.ToString(DateTimeFormat));

            Add(SenderName, dto.SenderName);
            Add(SenderAddress, dto.SenderAddress);
            Add(SenderEmail, dto.SenderEmail);      

            Add(RecipientName, dto.RecipientName);
            Add(RecipientAddress, dto.RecipientAddress);
            Add(RecipientEmail, dto.RecipientEmail);
            
            Add(InfringingUrl, dto.InfringingUrl);
            Add(AdditionalFacts, dto.AdditionalFacts);

            if (dto.YearOfCreation.HasValue)
            {
                Add(YearOfCreation, dto.YearOfCreation.Value.ToString());
            }   

            if (dto.DateOfPublication.HasValue)
            {
                Add(DateOfPublication, dto.DateOfPublication.Value.ToString(DateTimeFormat));
            }

            Add(NationOfFirstPublication, dto.NationOfFirstPublication);
            Add(GoodFaithStatement, dto.GoodFaithStatement);

            return tokens;
        }
    }
}
