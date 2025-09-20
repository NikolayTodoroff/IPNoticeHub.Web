using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Web.Models;
using IPNoticeHub.Web.Models.Trademarks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;

namespace IPNoticeHub.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPNoticeHubDbContext dbContext;

        public HomeController(IPNoticeHubDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> Results(string? trademark, TrademarkClass? classNumber, TrademarkStatusCategory? status,
        //TrademarkSearchBy? searchByItem, DataProvider? office, SearchMode? mode,int currentPage = 1, int pageSize = DefaultPageSize)
        //{
        //    var searchTerm = (trademark ?? string.Empty).Trim();
        //    var searchByFilter = searchByItem ?? TrademarkSearchBy.Wordmark;

        //    IQueryable<TrademarkEntity> q = dbContext.TrademarkRegistrations.AsNoTracking();
  
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
