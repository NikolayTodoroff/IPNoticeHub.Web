using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPNoticeHub.Data.Entities
{
    public class TrademarkClassAssignment
    {
        [Comment("The Nice Classification number (1–45) assigned to this trademark.")]
        public int ClassNumber { get; set; }


        [ForeignKey(nameof(TrademarkRegistration))]
        public int TrademarkRegistrationId { get; set; }
        public TrademarkRegistration TrademarkRegistration { get; set; } = null!;
    }
}
