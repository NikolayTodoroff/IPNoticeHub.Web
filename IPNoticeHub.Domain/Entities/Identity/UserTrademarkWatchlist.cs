using IPNoticeHub.Domain.Entities.Trademarks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Shared.Constants.
    ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Domain.Entities.Identity
{
    public sealed class UserTrademarkWatchlist
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(Trademark))]
        public int TrademarkId { get; set; }
        public TrademarkEntity Trademark { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public bool NotificationsEnabled { get; set; } = false;

        public DateTime AddedOnUtc { get; set; } = DateTime.UtcNow;

        public int? InitialStatusCodeRaw { get; set; }

        [MaxLength(WatchlistInitialStatusTextMaxLength)]
        public string? InitialStatusText { get; set; }

        public DateTime? InitialStatusDateUtc { get; set; }
    }
}
