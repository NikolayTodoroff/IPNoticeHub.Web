using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using IPNoticeHub.Data.Entities.TrademarkRegistration;

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


        [Comment("The UTC date and time when the trademark was added to the watchlist.")]
        public DateTime? WatchlistAddedOnDate { get; set; }


        [Comment("The initial status code of the trademark when it was added to the watchlist.")]
        public string? WatchlistInitialStatusCategory { get; set; }


        [Comment("Indicates whether notifications are enabled for this trademark on the watchlist.")]
        public bool WatchlistNotificationsEnabled { get; set; } = false;
    }
}
