using IPNoticeHub.Domain.Entities.Copyrights;

namespace IPNoticeHub.Domain.Entities.Identity
{
    public class UserCopyright
    {
        public string ApplicationUserId { get; set; } = string.Empty;


        public int CopyrightEntityId { get; set; }
        public CopyrightEntity CopyrightEntity { get; set; } = null!;


        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
