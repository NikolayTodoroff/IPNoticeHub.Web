using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Data.Entities.ApplicationUser
{
    public sealed class UserTrademarkWatchlist
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;


        [ForeignKey(nameof(Trademark))]
        public int TrademarkId { get; set; }
        public TrademarkEntity Trademark { get; set; } = null!;


        [Comment("Indicates whether the user has removed this registration from their watchlist (soft delete).")]
        public bool IsDeleted { get; set; }


        [Comment("Indicates whether notifications are enabled for this trademark on the watchlist.")]
        public bool NotificationsEnabled { get; set; } = true;


        [Comment("The UTC date and time when the trademark was added to the watchlist.")]
        public DateTime AddedOnUtc { get; set; } = DateTime.UtcNow;



        [Comment("The raw status code of the trademark as retrieved from the USPTO database or API.")]
        public int? InitialStatusCodeRaw { get; set; }


        [MaxLength(WatchlistInitialStatusTextMaxLength)]
        [Comment("The textual representation of the initial status of the trademark as retrieved from the USPTO database or API.")]
        public string? InitialStatusText { get; set; }


        [Comment("The UTC date and time when the initial status of the trademark was last updated or retrieved from the USPTO database or API.")]

        public DateTime? InitialStatusDateUtc { get; set; }
    }
}
