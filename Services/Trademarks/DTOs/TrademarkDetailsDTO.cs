using IPNoticeHub.Common.EnumConstants;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Services.Trademarks.DTOs
{
    public sealed class TrademarkDetailsDto
    {
        public int Id { get; init; }

        public Guid PublicId { get; init; }

        [Required, MaxLength(WordmarkMaxLength)]
        public string Wordmark { get; init; } = string.Empty;

        [Required, MaxLength(OwnerNameMaxLength)]
        public string? Owner { get; init; }

        [Required, MaxLength(SourceIdMaxLength)]
        public string SourceId { get; init; } = string.Empty;

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; init; }

        [Required]
        public TrademarkStatusCategory Status { get; init; }

        [MaxLength(GoodsAndServicesMaxLength)]
        public string? GoodsAndServices { get; init; }

        public DateTime? FilingDate { get; init; }

        public DateTime? RegistrationDate { get; init; }

        [MaxLength(MarkImageUrlMaxLength)]
        public string? MarkImageUrl { get; init; }

        public DataProvider Provider { get; init; }

        public IReadOnlyList<int> Classes { get; init; } = Array.Empty<int>();

        public IReadOnlyList<(DateTime Date, string Code, string? Description)> Events{ get; init; } = 
            Array.Empty<(DateTime, string, string?)>();
    }
}
