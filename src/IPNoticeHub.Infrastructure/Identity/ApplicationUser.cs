using Microsoft.AspNetCore.Identity;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.LegalDocuments;

namespace IPNoticeHub.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserTrademark> UserTrademarks { get; set; } = 
            new List<UserTrademark>();

        public ICollection<UserCopyright> UserCopyrights { get; set; } = 
            new List<UserCopyright>();

        public ICollection<LegalDocument> LegalDocuments { get; set; } = 
            new List<LegalDocument>();
    }
}
