using System.ComponentModel.DataAnnotations;
using IPNoticeHub.Common.EnumConstants;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Services.Trademarks.DTOs
{
    public sealed class TrademarkFilterDTO
    {
        public DataProvider? Provider { get; init; }

        public int[]? ClassNumbers { get; init; }

        public TrademarkStatusCategory? Status { get; init; }

        [Required]
        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;

        [StringLength(FilterSearchTermMaxLength)]
        public string? SearchTerm { get; init; }

        public bool ExactMatch { get; init; } = false;
    }
}
