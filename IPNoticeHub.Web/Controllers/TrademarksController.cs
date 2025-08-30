using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.Trademarks;
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
        public async Task<IActionResult> Index([FromQuery] TrademarkFilterViewModel filter)
        {
            string searchTerm = (filter.SearchTerm ?? string.Empty).Trim();
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                ViewBag.HasSearch = false;

                PagedResult<TrademarkSummaryDTO>? emptyPageDTO = new PagedResult<TrademarkSummaryDTO>()
                {
                    Results = Array.Empty<TrademarkSummaryDTO>(),
                    ResultsCount = 0,
                    CurrentPage = filter.CurrentPage,
                    ResultsCountPerPage = filter.ResultsPerPage
                };

                TrademarksIndexViewModel emptyPageViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, emptyPageDTO);
                return View(emptyPageViewModel);
            }


            if (!ModelState.IsValid)
            {
                ViewBag.HasSearch = false;

                PagedResult<TrademarkSummaryDTO>? invalidPageDTO = new PagedResult<TrademarkSummaryDTO>()
                {
                    Results = Array.Empty<TrademarkSummaryDTO>(),
                    ResultsCount = 0,
                    CurrentPage = filter.CurrentPage,
                    ResultsCountPerPage = filter.ResultsPerPage
                };

                TrademarksIndexViewModel? invalidPageViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, invalidPageDTO);
                return View(invalidPageViewModel);
            }

            TrademarkFilterDTO filterDTO = new TrademarkFilterDTO
            {
                SearchTerm = searchTerm,
                SearchBy = filter.SearchBy,
                Provider = filter.Provider,
                Status = filter.Status,
                ClassNumbers = (filter.ClassNumbers ?? Array.Empty<int>())
                        .Where(cn => Enum.IsDefined(typeof(TrademarkClass), cn))
                        .Distinct()
                        .ToArray(),
                ExactMatch = filter.ExactMatch
            };

            PagedResult<TrademarkSummaryDTO>? resultsPageDTO = await searchService.SearchAsync(filterDTO, filter.CurrentPage, filter.ResultsPerPage);

            ViewBag.HasSearch = true;

            TrademarksIndexViewModel? indexViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, resultsPageDTO);
            return View(indexViewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            TrademarkDetailsDTO? detailsModel = await searchService.GetDetailsAsync(id);

            if (detailsModel is null) return NotFound();

            return View(detailsModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyCollection(int currentPage = DefaultPage, int pageSize = DefaultPageSize)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            PagedResult<TrademarkSummaryDTO>? collectionModel = await collectionService.GetUserCollectionAsync(userId, currentPage, pageSize);
            return View(collectionModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,int currentPage = DefaultPage,int pageSize = DefaultPageSize)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            PagedResult<TrademarkSummaryDTO>? orderedCollectionModel = await collectionService.GetUserCollectionAsync (userId, sortBy, currentPage, pageSize);

            ViewBag.SortBy = sortBy;
            return View(orderedCollectionModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            await collectionService.AddAsync(userId, trademarkId);

            TempData["StatusMessage"] = "Trademark added to your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            await collectionService.RemoveAsync(userId, trademarkId);

            TempData["StatusMessage"] = "Trademark removed from your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}
