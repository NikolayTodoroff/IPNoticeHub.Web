using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Web.Models.TrademarkCollection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    [AutoValidateAntiforgeryToken]
    [Route("Trademarks/Collection")]
    public class TrademarkCollectionController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;

        public TrademarkCollectionController(ITrademarkCollectionService tmCollectionService)
        {
            this.tmCollectionService = tmCollectionService;
        }

        [HttpGet]
        public async Task<IActionResult>Index(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, int resultsPerPage = DefaultPageSize, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            PagedResult<Services.Trademarks.DTOs.TrademarkSummaryDTO> dtoPagedResult =
                await tmCollectionService.GetUserCollectionAsync(userId, currentPage, resultsPerPage, cancellationToken);

            var indexViewModel = TrademarkCollectionDtoToVmMapper.Map(dtoPagedResult);

            ViewBag.SortBy = sortBy;
            return View("TrademarkCollection", indexViewModel);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.AddAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = TrademarkAddedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(Index));
        }

        [HttpPost("Remove")]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.RemoveAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = TrademarkRemovedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(Index));
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            return (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) ? Redirect(returnUrl) : null;
        }
    }
}
