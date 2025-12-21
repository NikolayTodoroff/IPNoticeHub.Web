using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Application.LetterComposition.PlaceholderBuilders
{
    public class CopyrightCadPlaceholderBuilder
    {
        public static IDictionary<string, string> BuildPlaceholders(CopyrightCadDraftDto dto)
        {
            var placeholders = 
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WorkTitle"] = dto.WorkTitle ?? string.Empty,
                ["WorkDescription"] = dto.RegistrationNumber ?? string.Empty,

                ["SenderName"] = dto.SenderName ?? string.Empty,
                ["SenderAddress"] = dto.SenderAddress ?? string.Empty,
                ["SenderEmail"] = dto.SenderEmail ?? string.Empty,

                ["RecipientName"] = dto.RecipientName ?? string.Empty,
                ["RecipientAddress"] = dto.RecipientAddress ?? string.Empty,
                ["RecipientEmail"] = dto.RecipientEmail ?? string.Empty,

                ["InfringingUrls"] = dto.InfringingUrl ?? string.Empty,
                ["AdditionalDetails"] = dto.AdditionalFacts ?? string.Empty
            };  

            return placeholders;
        }
    }
}
