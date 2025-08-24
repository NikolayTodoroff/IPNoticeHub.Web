using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<TrademarkRegistration> Trademarks { get; set; } = new List<TrademarkRegistration>();
    }
}
