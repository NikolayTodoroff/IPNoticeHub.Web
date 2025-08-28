using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Abstractions;
using IPNoticeHub.Services.DTOs.Trademarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IPNoticeHub.Common.EntityValidationConstants.PagingConstants;

namespace IPNoticeHub.Web.Controllers
{
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService collectionService;
        private readonly ITrademarkSearchService searchService;

        public TrademarksController(ITrademarkSearchService searchService, ITrademarkCollectionService collectionService)
        {
            this.searchService = searchService;
            this.collectionService = collectionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TrademarkFilterDTO filter, int currentPage = DefaultPage, int pageSize = DefaultPageSize)
        {
            if (string.IsNullOrWhiteSpace(filter?.SearchTerm) || !ModelState.IsValid)
            {
                ViewBag.HasSearch = false;

                // Return an empty PagedResult so the view can bind safely
                var emptyModel = new PagedResult<TrademarkListItemDTO>
                {
                    Results = Array.Empty<TrademarkListItemDTO>(),
                    ResultsCount = 0,
                    CurrentPage = currentPage,
                    ResultsCountPerPage = pageSize
                };

                return View(emptyModel);
            }

            var searchResultModel = await searchService.SearchAsync(filter, currentPage, pageSize);
            ViewBag.HasSearch = true;
            return View(searchResultModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var detailsModel = await searchService.GetDetailsAsync(id);

            if (detailsModel is null)
            {
                return NotFound();
            } 

            return View(detailsModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyCollection(int currentPage = DefaultPage, int pageSize = DefaultPageSize)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Challenge();
            } 

            var collectionModel = await collectionService.GetUserCollectionAsync(userId, currentPage, pageSize);
            return View(collectionModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyCollection(TrademarkCollectionSortBy sortBy = TrademarkCollectionSortBy.DateAddedDesc,int currentPage = DefaultPage,int pageSize = DefaultPageSize)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Challenge();
            } 

            var orderedCollectionModel = await collectionService.GetUserCollectionAsync(
                userId, sortBy, currentPage, pageSize);

            ViewBag.SortBy = sortBy;
            return View(orderedCollectionModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Challenge();
            }

            await collectionService.AddAsync(userId, trademarkId);

            TempData["StatusMessage"] = "Trademark added to your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Challenge();
            }

            await collectionService.RemoveAsync(userId, trademarkId);

            TempData["StatusMessage"] = "Trademark removed from your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}
