using Humanizer;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.Trademarks;
using IPNoticeHub.Web.ViewModels.Trademarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using IPNoticeHub.Web.Infrastructure;

namespace IPNoticeHub.Web.Controllers
{
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;
        private readonly ITrademarkSearchService tmSearchService;
        private readonly ITrademarkWatchlistService tmWatchlistService;

        public TrademarksController(ITrademarkSearchService searchService, ITrademarkCollectionService collectionService, ITrademarkWatchlistService tmWatchlistService)
        {
            this.tmSearchService = searchService;
            this.tmCollectionService = collectionService;
            this.tmWatchlistService = tmWatchlistService;
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

            PagedResult<TrademarkSummaryDTO> dtoPagedResult = await tmSearchService.SearchAsync(filterDTO, filter.CurrentPage, filter.ResultsPerPage, cancellationToken);

            ViewBag.HasSearch = true;

            TrademarksIndexViewModel indexViewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, dtoPagedResult);
            return View(indexViewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            TrademarkDetailsDTO? detailsDTO = await tmSearchService.GetDetailsAsync(id, cancellationToken);

            if (detailsDTO is null) return NotFound();
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var detailsViewModel = new TrademarkDetailsViewModel
            {
                Id = detailsDTO.Id,
                PublicId = detailsDTO.PublicId,
                Wordmark = detailsDTO.Wordmark,
                Owner = detailsDTO.Owner,
                SourceId = detailsDTO.SourceId,
                RegistrationNumber = detailsDTO.RegistrationNumber,
                Status = detailsDTO.Status,
                GoodsAndServices = detailsDTO.GoodsAndServices,
                FilingDate = detailsDTO.FilingDate,
                RegistrationDate = detailsDTO.RegistrationDate,
                MarkImageUrl = detailsDTO.MarkImageUrl,
                Provider = detailsDTO.Provider,
                Classes = detailsDTO.Classes,
                IsInCollection = await tmCollectionService.IsInCollectionAsync(userId, detailsDTO.Id, false, cancellationToken),
                IsInWatchlist = await tmWatchlistService.ExistsAsync(userId, detailsDTO.Id, cancellationToken)
            };

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                ViewBag.ReturnUrl = returnUrl;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }

            return View(detailsViewModel);
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

            TempData["SuccessMessage"] = TmAddedToCollectionMessage;
            return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.RemoveAsync(userId, trademarkId, cancellationToken);

            TempData["SuccessMessage"] = TmRemovedFromCollectionMessage;
            return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
        }

        // Helper methods for internal use within the controller
        private IActionResult CreateEmptyViewModel(TrademarkFilterViewModel filter)
        {
            ViewBag.HasSearch = false;
            PagedResult<TrademarkSummaryDTO>? emptyDTOPagedResult = new PagedResult<TrademarkSummaryDTO>
            {
                Results = Array.Empty<TrademarkSummaryDTO>(),
                ResultsCount = 0,
                CurrentPage = filter.CurrentPage,
                ResultsCountPerPage = filter.ResultsPerPage
            };

            TrademarksIndexViewModel? viewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, emptyDTOPagedResult);
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
    }
}
