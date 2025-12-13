using System.ComponentModel.DataAnnotations;
using IPNoticeHub.Shared.Enums;
using static IPNoticeHub.Shared.Constants.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Application.Trademarks.DTOs
{
    public sealed class TrademarkFilterDto
    {
        public DataProvider? Provider { get; init; }

        public int[]? ClassNumbers { get; init; }

        public TrademarkStatusCategory? Status { get; init; }

        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;

        [StringLength(FilterSearchTermMaxLength)]
        public string? SearchTerm { get; init; }

        public bool ExactMatch { get; init; } = false;
    }
}
