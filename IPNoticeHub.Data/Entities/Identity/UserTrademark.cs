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


        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
