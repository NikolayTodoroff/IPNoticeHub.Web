using IPNoticeHub.Data.Entities.Identity;

namespace IPNoticeHub.Web.Models.AdminDashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }


        public int TrademarksAdded { get; set; }


        public int CopyrightsAdded { get; set; }


        public int WatchlistedItems { get; set; }


        public ICollection<ApplicationUser> RecentRegistrations { get; set; } = 
            new List<ApplicationUser>();
    }
}
