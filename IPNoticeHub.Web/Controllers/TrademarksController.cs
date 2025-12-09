using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.PdfGeneration.Abstractions;
using IPNoticeHub.Services.Watchlist.Abstractions;

namespace IPNoticeHub.Web.Controllers
{
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;
        private readonly ITrademarkSearchService tmSearchService;
        private readonly ITrademarkWatchlistService tmWatchlistService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;

        public TrademarksController(
            ITrademarkSearchService searchService, 
            ITrademarkCollectionService collectionService, 
            ITrademarkWatchlistService tmWatchlistService, 
            IPdfService pdfService, 
            ILetterTemplateProvider letterTemplateProvider, 
            IDocumentLibraryService documentLibraryService)
        {
            this.tmSearchService = searchService;
            this.tmCollectionService = collectionService;
            this.tmWatchlistService = tmWatchlistService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet]
        public async Task<IActionResult> MyCollection(
            CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, 
            int resultsPerPage = DefaultPageSize, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await tmCollectionService.
                GetUserCollectionAsync(userId, sortBy, currentPage, resultsPerPage, 
                cancellationToken);

            var viewModel = 
                TrademarksMapping.MapCollectionDtoToViewModel(dto);

            ViewBag.SortBy = sortBy;

            return View(viewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(
            Guid id, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            var dto = await tmSearchService.
                GetDetailsAsync(id, cancellationToken);

            if (dto is null) return NotFound();

            bool isAuthenticated = User.Identity?.IsAuthenticated == true;
            bool isInCollection = false;
            bool isInWatchlist = false;

            if (isAuthenticated && User.TryGetUserId(out var userId))
            {
                isInCollection = await tmCollectionService.
                    IsInCollectionAsync(userId, dto.Id,false,
                    cancellationToken);

                isInWatchlist = await tmWatchlistService.
                    ExistsAsync(userId, dto.Id, 
                    cancellationToken);
            }

            var viewModel = 
                TrademarksMapping.MapDetailsDtoToViewModel(
                    dto,
                    isInCollection,
                    isInWatchlist,
                    isAuthenticated);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }

            return View(viewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            int trademarkId, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            try
            {
                if (await tmCollectionService.IsInCollectionAsync(
                    userId, trademarkId, false, cancellationToken))
                {
                    TempData["InfoMessage"] = "Trademark is already in your collection.";
                    return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
                }

                await tmCollectionService.AddAsync(
                    userId, 
                    trademarkId, 
                    cancellationToken);

                TempData["SuccessMessage"] = TmAddedToCollectionMessage;
                return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
            }

            catch
            {
                TempData["ErrorMessage"] = TmAddToCollectionErrorMessage;
                return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
            }
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int trademarkId, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.RemoveAsync(
                userId, 
                trademarkId, 
                cancellationToken);

            TempData["SuccessMessage"] = TmRemovedFromCollectionMessage;
            return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
        }
    }
}
