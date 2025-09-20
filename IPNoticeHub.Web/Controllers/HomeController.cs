using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using IPNoticeHub.Web.Models.Trademarks;

namespace IPNoticeHub.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Results(string? tmQuery, TrademarkClass? tmClass, TrademarkStatusCategory? tmStatus,
        TrademarkSearchBy? tmSearchItem, DataProvider? tmOffice, SearchMode? tmMode,int currentPage = 1, int pageSize = 25)
        {
            var resultViewModel = new TrademarkSearchResultsViewModel
            {
                Query = tmQuery,
                Class = tmClass,
                Status = tmStatus,
                SearchBy = tmSearchItem,
                Office = tmOffice,
                Mode = tmMode,
                Results = Enumerable.Empty<TrademarkResultRowViewModel>(),
                Total = 0
            };
            return View(resultViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
