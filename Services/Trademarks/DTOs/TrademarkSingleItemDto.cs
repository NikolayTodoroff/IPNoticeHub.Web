using IPNoticeHub.Common.EnumConstants;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Services.Trademarks.DTOs
{
    public sealed class TrademarkSingleItemDto
    {
        public int Id { get; init; }


        public Guid PublicId { get; init; }


        [Required, MaxLength(WordmarkMaxLength)]
        public string Wordmark { get; init; } = string.Empty;


        [Required, MaxLength(SourceIdMaxLength)]
        public string SourceId { get; init; } = string.Empty;


        [MaxLength(OwnerNameMaxLength)]
        public string? Owner { get; init; }
        

        public TrademarkStatusCategory Status { get; init; }


        public int[] Classes { get; init; } = Array.Empty<int>();


        public DataProvider Provider { get; init; }
    }
}
