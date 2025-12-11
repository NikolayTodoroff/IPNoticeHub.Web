using IPNoticeHub.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Shared.Constants.StatusMessages.TrademarkStatusMessages;
using static IPNoticeHub.Shared.Constants.StatusMessages.EmailNotificationsStatusMessages;
using IPNoticeHub.Web.Models.Watchlist;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Web.WebHelpers;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class WatchlistController : Controller
    {
        private readonly IWatchlistService watchlistService;

        public WatchlistController(IWatchlistService watchlistService)
        {
            this.watchlistService = watchlistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            var watchlistItems = 
                await watchlistService.GetListByUserAsync(
                    userId, 
                    cancellationToken);

            var watchlistViewModel = 
                new WatchlistIndexViewModel
            {
                Items = watchlistItems
            };

            return View(watchlistViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int trademarkId, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                if (await watchlistService.ExistsAsync(
                    userId, 
                    trademarkId, 
                    cancellationToken))
                {
                    TempData["InfoMessage"] = TmAlreadyInWatchlistMessage;
                    return this.RedirectToLocalOrAction(
                        returnUrl, 
                        nameof(Index));
                }

                await watchlistService.AddAsync(
                    userId, 
                    trademarkId, 
                    cancellationToken);

                TempData["SuccessMessage"] = TmAddedToWatchlistMessage;

                return this.RedirectToLocalOrAction(
                    returnUrl, 
                    nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = TmAddToWatchlistErrorMessage;

                return this.RedirectToLocalOrAction(
                    returnUrl, 
                    nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int trademarkId, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            await watchlistService.RemoveAsync(
                userId, 
                trademarkId, 
                cancellationToken);

            TempData["SuccessMessage"] = TmRemovedFromWatchlistMessage;

            return this.RedirectToLocalOrAction(
                returnUrl, 
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNotifications(
            int trademarkId, 
            bool enabled, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Unauthorized();

            try
            {
                await watchlistService.ToggleNotificationsAsync(
                    userId, 
                    trademarkId, 
                    enabled, 
                    cancellationToken);

                TempData["SuccessMessage"] = enabled ? 
                    EmailNotificationsEnabledMessage : EmailNotificationsDisabledMessage;
            }
            catch
            {
                TempData["ErrorMessage"] = EmailNotificationsErrorMessage;
            }

            return this.RedirectToLocalOrAction(
                returnUrl, 
                nameof(Index));
        }
    }
}
