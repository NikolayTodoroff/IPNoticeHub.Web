using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using IPNoticeHub.Data.Entities.TrademarkRegistration;

namespace IPNoticeHub.Data.Entities.Identity
{
    public class UserTrademark
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;


        [ForeignKey(nameof(TrademarkRegistration))]
        public int TrademarkId { get; set; }
        public TrademarkEntity Trademark { get; set; } = null!;


        [Comment("Date when the user added this trademark registration to their account")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;


        [Comment("Indicates whether the user has removed this registration from their collection (soft delete).")]
        public bool IsDeleted { get; set; } = false;
    }
}
