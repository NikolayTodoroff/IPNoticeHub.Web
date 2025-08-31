using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.Trademarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using IPNoticeHub.Common.Extensions;

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
        public async Task<IActionResult> Index([FromQuery] TrademarkFilterViewModel filter, CancellationToken cancellationToken)
        {
            string searchTerm = (filter.SearchTerm ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(searchTerm) || !ModelState.IsValid)
            {
                return CreateEmptyViewModel(filter);
            }

            TrademarkFilterDTO? filterDTO = CreateNormalizedFilterDTO(filter, searchTerm);

            PagedResult<TrademarkSummaryDTO> resultsPageDTO = await searchService.SearchAsync(filterDTO, filter.CurrentPage, filter.ResultsPerPage, cancellationToken);

            ViewBag.HasSearch = true;

            TrademarksIndexViewModel indexViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, resultsPageDTO);
            return View(indexViewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            TrademarkDetailsDTO? detailsModel = await searchService.GetDetailsAsync(id, cancellationToken);

            if (detailsModel is null) return NotFound();

            return View(detailsModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
        int currentPage = DefaultPage,int resultsPerPage = DefaultPageSize,CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var page = await collectionService.GetUserCollectionAsync(userId, sortBy, currentPage, resultsPerPage, cancellationToken);
            ViewBag.SortBy = sortBy;
            return View(page);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await collectionService.AddAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = "Trademark added to your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await collectionService.RemoveAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = "Trademark removed from your collection.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        // Helper methods for internal use within the controller
        private IActionResult CreateEmptyViewModel(TrademarkFilterViewModel filter)
        {
            ViewBag.HasSearch = false;
            var emptyPageDTO = new PagedResult<TrademarkSummaryDTO>
            {
                Results = Array.Empty<TrademarkSummaryDTO>(),
                ResultsCount = 0,
                CurrentPage = filter.CurrentPage,
                ResultsCountPerPage = filter.ResultsPerPage
            };
            var viewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, emptyPageDTO);
            return View(viewModel);
        }
        private static TrademarkFilterDTO CreateNormalizedFilterDTO(TrademarkFilterViewModel filter, string searchTerm)
        {
            return new TrademarkFilterDTO
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
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}
