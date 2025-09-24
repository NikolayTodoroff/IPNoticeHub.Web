using System.Security.Claims;
using IPNoticeHub.Services.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class WatchlistController : Controller
    {
        private readonly ITrademarkWatchlistService watchlistService;

        public WatchlistController(ITrademarkWatchlistService watchlistService)
        {
            this.watchlistService = watchlistService;
        }
        //[HttpGet]
        //public async Task<IActionResult> Index(CancellationToken ct)
        //{
        //    var userId = GetUserId();
        //    if (userId is null) return Unauthorized();

        //    var items = await watchlistService.GetListByUserAsync(userId, ct);
        //    return View(items);
        //}
    }
}
