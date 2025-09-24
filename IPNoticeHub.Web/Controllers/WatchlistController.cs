using System.Security.Claims;
using IPNoticeHub.Services.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPNoticeHub.Common.Infrastructure;

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

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            var watchlistItems = await watchlistService.GetListByUserAsync(userId, cancellationToken);
            return View(watchlistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                await watchlistService.AddAsync(userId, trademarkId, cancellationToken);
                TempData["Success"] = "Added to Watchlist.";
            }

            catch (Exception)
            {
                TempData["Error"] = "Could not add to Watchlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            await watchlistService.RemoveAsync(userId, trademarkId, ct);
            TempData["Success"] = "Removed from watchlist.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNotifications(int trademarkId, bool enabled, CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                await watchlistService.ToggleNotificationsAsync(userId, trademarkId, enabled, ct);
                TempData["Success"] = enabled 
                    ? "Notifications enabled successfully."
                    : "Notifications disabled successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to toggle notifications.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
