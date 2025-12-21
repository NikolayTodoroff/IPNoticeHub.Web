using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Application.DTOs.DraftStoreDTOs
{
    public class CopyrightCadDraftDto
    {
        public string? SenderName { get; init; }
        public string? SenderOrganization { get; init; }
        public string? SenderAddress { get; init; }
        public string? SenderEmail { get; init; }
        public string? SenderPhone { get; init; }

        public string? RecipientName { get; init; }
        public string? RecipientOrganization { get; init; }
        public string? RecipientAddress { get; init; }
        public string? RecipientEmail { get; init; }

        public string? WorkTitle { get; init; }
        public string? WorkDescription { get; init; }

        public string? InfringingUrls { get; init; }
        public string? OriginalWorkUrl { get; init; }

        public string? AdditionalDetails { get; init; }
        public string? SignatureName { get; init; }
        public DateTimeOffset? SignatureDateUtc { get; init; }

        public Dictionary<string, string>? Extra { get; init; }
    }
}
