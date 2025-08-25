using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserTrademark> UserTrademarks { get; set; } = new List<UserTrademark>();
        public ICollection<UserCopyright> UserCopyrights { get; set; } = new List<UserCopyright>();
    }
}
