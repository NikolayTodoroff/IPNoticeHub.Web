using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.Trademarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPNoticeHub.Common.Extensions;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Web.Controllers
{
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;
        private readonly ITrademarkSearchService tmSearchService;

        public TrademarksController(ITrademarkSearchService searchService, ITrademarkCollectionService collectionService)
        {
            this.tmSearchService = searchService;
            this.tmCollectionService = collectionService;
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

            PagedResult<TrademarkSummaryDTO> resultsPageDTO = await tmSearchService.SearchAsync(filterDTO, filter.CurrentPage, filter.ResultsPerPage, cancellationToken);

            ViewBag.HasSearch = true;

            TrademarksIndexViewModel indexViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, resultsPageDTO);
            return View(indexViewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            TrademarkDetailsDTO? detailsModel = await tmSearchService.GetDetailsAsync(id, cancellationToken);

            if (detailsModel is null) return NotFound();

            return View(detailsModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
        int currentPage = DefaultPage,int resultsPerPage = DefaultPageSize,CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var page = await tmCollectionService.GetUserCollectionAsync(userId, sortBy, currentPage, resultsPerPage, cancellationToken);
            ViewBag.SortBy = sortBy;
            return View(page);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.AddAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = TrademarkAddedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.RemoveAsync(userId, trademarkId, cancellationToken);

            TempData["StatusMessage"] = TrademarkRemovedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        // Helper methods for internal use within the controller
        private IActionResult CreateEmptyViewModel(TrademarkFilterViewModel filter)
        {
            ViewBag.HasSearch = false;
            PagedResult<TrademarkSummaryDTO>? emptyPageDTO = new PagedResult<TrademarkSummaryDTO>
            {
                Results = Array.Empty<TrademarkSummaryDTO>(),
                ResultsCount = 0,
                CurrentPage = filter.CurrentPage,
                ResultsCountPerPage = filter.ResultsPerPage
            };

            TrademarksIndexViewModel? viewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, emptyPageDTO);
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
            {
                return Redirect(returnUrl);
            }       

            return null;
        }
    }
}
