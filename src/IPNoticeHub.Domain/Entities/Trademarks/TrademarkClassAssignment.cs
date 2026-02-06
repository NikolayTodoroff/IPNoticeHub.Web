using System.ComponentModel.DataAnnotations.Schema;

namespace IPNoticeHub.Domain.Entities.Trademarks
{
    public class TrademarkClassAssignment
    {
        public int ClassNumber { get; set; }

        [ForeignKey(nameof(TrademarkRegistration))]
        public int TrademarkRegistrationId { get; set; }

        public TrademarkEntity TrademarkRegistration { get; set; } = null!;
    }
}
