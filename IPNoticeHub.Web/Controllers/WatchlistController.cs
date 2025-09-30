using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Web.Infrastructure;
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

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            var watchlistItems = await watchlistService.GetListByUserAsync(userId, cancellationToken);

            var watchlistViewModel = new WatchlistIndexViewModel
            {
                Items = watchlistItems
            };

            return View(watchlistViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                if (await watchlistService.ExistsAsync(userId, trademarkId, cancellationToken))
                {
                    TempData["Info"] = "Trademark is already in your watchlist.";
                    return this.RedirectToLocalOrAction(returnUrl, nameof(Index));
                }

                await watchlistService.AddAsync(userId, trademarkId, cancellationToken);
                TempData["Success"] = "Added to Watchlist.";
                return this.RedirectToLocalOrAction(returnUrl, nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Could not add to Watchlist.";
                return this.RedirectToLocalOrAction(returnUrl, nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            await watchlistService.RemoveAsync(userId, trademarkId, cancellationToken);
            TempData["Success"] = "Removed from watchlist.";
            return this.RedirectToLocalOrAction(returnUrl, nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNotifications(int trademarkId, bool enabled, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                await watchlistService.ToggleNotificationsAsync(userId, trademarkId, enabled, cancellationToken);
                TempData["Success"] = enabled
                    ? "Email notifications enabled successfully."
                    : "Email notifications disabled successfully.";
            }
            catch
            {
                TempData["Error"] = "Failed to toggle email notifications.";
            }

            return this.RedirectToLocalOrAction(returnUrl, nameof(Index));
        }
    }
}
