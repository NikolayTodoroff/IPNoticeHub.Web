using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Services.Application.DTOs
{
    public sealed class TrademarkSearchResultDto
    {
        public int Id { get; init; }


        public Guid PublicId { get; init; }


        [MaxLength(RegistrationNumberMaxLength)]
        public string RegistrationNumber { get; init; } = string.Empty;


        [Required, MaxLength(WordmarkMaxLength)]
        public string Wordmark { get; init; } = string.Empty;


        [Required, MaxLength(OwnerNameMaxLength)]
        public string Owner { get; init; } = string.Empty;


        [Required, MaxLength(TrademarkStatusDetailsMaxLength)]
        public string Status { get; init; } = string.Empty;


        public DateTime? RegistrationDate { get; init; }
    }
}
