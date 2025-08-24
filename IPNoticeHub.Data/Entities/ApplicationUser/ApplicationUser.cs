using Microsoft.AspNetCore.Identity;

namespace IPNoticeHub.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Trademark> Trademarks { get; set; } = new List<Trademark>();
    }
}
