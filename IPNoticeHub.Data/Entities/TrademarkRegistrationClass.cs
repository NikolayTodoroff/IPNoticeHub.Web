using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.EnumConstants;

namespace IPNoticeHub.Data.Entities
{
    public class TrademarkRegistrationClass
    {
        [Comment("Foreign key referencing the TrademarkRegistration entity")]
        public int TrademarkRegistrationId { get; set; }

        [Comment("Navigation property for the related TrademarkRegistration entity")]
        public TrademarkRegistration Registration { get; set; } = null!;

        [Comment("Trademark class associated with the registration")]
        public TrademarkClass Class { get; set; }
    }
}