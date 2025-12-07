using Microsoft.AspNetCore.Identity;
using IPNoticeHub.Data.Entities.LegalDocuments;

namespace IPNoticeHub.Data.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserTrademark> UserTrademarks { get; set; } = 
            new List<UserTrademark>();

        public ICollection<UserCopyright> UserCopyrights { get; set; } = 
            new List<UserCopyright>();

        public ICollection<LegalDocument> Documents { get; set; } = 
            new List<LegalDocument>();
    }
}
