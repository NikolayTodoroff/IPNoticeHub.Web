using IPNoticeHub.Data.Entities.CopyrightRegistration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPNoticeHub.Data.Entities.ApplicationUser
{
    public class UserCopyright
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;


        [ForeignKey(nameof(CopyrightRegistration))]
        public int CopyrightRegistrationId { get; set; }
        public CopyrightEntity CopyrightRegistration { get; set; } = null!;


        [Comment("Date when the user added this copyright registration to their account")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;


        [Comment("Indicates whether the user soft-deleted this from their collection/watchlist")]
        public bool IsDeleted { get; set; } = false;
    }
}
