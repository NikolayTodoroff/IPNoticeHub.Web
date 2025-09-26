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

            var watchlistViewModel = new WatchlistIndexViewModel
            {
                Items = watchlistItems
            };

            return View(watchlistViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl, CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                if (await watchlistService.ExistsAsync(userId, trademarkId, cancellationToken))
                {
                    TempData["Info"] = "Trademark is already in your watchlist.";

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction(nameof(Index));
                }

                await watchlistService.AddAsync(userId, trademarkId, cancellationToken);
                TempData["Success"] = "Added to Watchlist.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Could not add to Watchlist.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

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
        public async Task<IActionResult> ToggleNotifications(int trademarkId, bool notificationsEnabled, CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                await watchlistService.ToggleNotificationsAsync(userId, trademarkId, notificationsEnabled, cancellationToken);
                TempData["Success"] = notificationsEnabled
                    ? "Email notifications enabled successfully."
                    : "Email notifications disabled successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to toggle email notifications.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
