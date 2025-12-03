using IPNoticeHub.Data;
using IPNoticeHub.Web.Models.AdminDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly IPNoticeHubDbContext dbContext;

        public AdminController(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = dbContext.Users.Count(),
                TrademarksAdded = dbContext.TrademarkRegistrations.Count(),
                CopyrightsAdded = dbContext.CopyrightRegistrations.Count(),
                WatchlistedItems = dbContext.UserTrademarkWatchlists.Count(),

                RecentRegistrations = dbContext.Users.
                OrderByDescending(u=>u.Id).
                Take(5).
                ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Sync()
        {
            TempData["SuccessMessage"] = ManualSyncTriggeredMessage;
            return RedirectToAction("Index");
        }
    }
}
