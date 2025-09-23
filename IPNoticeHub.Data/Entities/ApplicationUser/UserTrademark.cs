using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;

namespace IPNoticeHub.Data.Entities.ApplicationUser
{
    public class UserTrademark
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;


        [ForeignKey(nameof(TrademarkRegistration))]
        public int TrademarkRegistrationId { get; set; }
        public TrademarkEntity TrademarkRegistration { get; set; } = null!;


        [Comment("Date when the user added this trademark registration to their account")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;


        [Comment("Indicates whether the user has removed this registration from their collection or watchlist (soft delete).")]
        public bool IsDeleted { get; set; } = false;


        // Watchlist Specific
        [Comment("Indicates whether the trademark is on a watchlist")]
        public bool AddedToWatchlist { get; set; } = false;


        [Comment("Indicates whether notifications are enabled for this trademark on the watchlist.")]
        public bool WatchlistNotificationsEnabled { get; set; } = false;


        [Comment("The UTC date and time when the trademark was added to the watchlist.")]
        public DateTime? WatchlistAddedOnUtc { get; set; }


        [Comment("The raw status code of the trademark as retrieved from the USPTO database or API.")]
        public int? WatchlistInitialStatusCodeRaw { get; set; }


        [MaxLength(WatchlistInitialStatusTextMaxLength)]
        [Comment("The textual representation of the initial status of the trademark as retrieved from the USPTO database or API.")]
        public string? WatchlistInitialStatusText { get; set; }


        [Comment("Indicates whether the initial status date of the trademark was retrieved from the USPTO database or API.")]
        public DateTime? WatchlistInitialStatusDateUtc { get; set; }
    }
}
