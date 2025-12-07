using IPNoticeHub.Data.Entities.CopyrightRegistration;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPNoticeHub.Data.Entities.Identity
{
    public class UserCopyright
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;


        [ForeignKey(nameof(CopyrightRegistration))]
        public int CopyrightRegistrationId { get; set; }
        public CopyrightEntity CopyrightRegistration { get; set; } = null!;


        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
