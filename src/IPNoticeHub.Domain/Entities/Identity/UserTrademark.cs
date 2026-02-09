using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Domain.Entities.Identity
{
    public class UserTrademark
    {
        public string ApplicationUserId { get; set; } = null!;

        public int TrademarkEntityId { get; set; }
        public TrademarkEntity TrademarkEntity { get; set; } = null!;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
