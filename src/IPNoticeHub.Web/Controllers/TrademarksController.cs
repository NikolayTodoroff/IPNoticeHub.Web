using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Shared.Constants.StatusMessages.TrademarkStatusMessages;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Web.WebHelpers.Mappings;
using IPNoticeHub.Web.WebHelpers;
using IPNoticeHub.Application.Templates.Abstractions;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;
        private readonly ITrademarkSearchService tmSearchService;
        private readonly IWatchlistService tmWatchlistService;
        private readonly IPdfLetterService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;

        public TrademarksController(
            ITrademarkSearchService searchService, 
            ITrademarkCollectionService collectionService, 
            IWatchlistService tmWatchlistService,
            IPdfLetterService pdfService, 
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

        [HttpGet]
        public async Task<IActionResult> MyCollection(
            CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, 
            int resultsPerPage = DefaultPageSize, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await tmCollectionService.
                GetUserCollectionAsync(
                userId, 
                sortBy, 
                currentPage, 
                resultsPerPage, 
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
                GetDetailsAsync(
                id, 
                cancellationToken);

            if (dto is null) return NotFound();

            bool isAuthenticated = User.Identity?.IsAuthenticated == true;
            bool isInCollection = false;
            bool isInWatchlist = false;

            if (isAuthenticated && User.TryGetUserId(out var userId))
            {
                isInCollection = await tmCollectionService.
                    IsInCollectionAsync(
                    userId, 
                    dto.Id,
                    false,
                    cancellationToken);

                isInWatchlist = await tmWatchlistService.
                    ExistsAsync(userId, 
                    dto.Id, 
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
                    userId, 
                    trademarkId, 
                    false, 
                    cancellationToken))
                {
                    TempData["InfoMessage"] = "Trademark is already in your collection.";

                    return this.RedirectToLocalOrAction(
                        returnUrl, 
                        nameof(MyCollection));
                }

                await tmCollectionService.AddAsync(
                    userId, 
                    trademarkId, 
                    cancellationToken);

                TempData["SuccessMessage"] = TmAddedToCollectionMessage;

                return this.RedirectToLocalOrAction(
                    returnUrl, 
                    nameof(MyCollection));
            }

            catch
            {
                TempData["ErrorMessage"] = TmAddToCollectionErrorMessage;

                return this.RedirectToLocalOrAction(
                    returnUrl, 
                    nameof(MyCollection));
            }
        }

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

            return this.RedirectToLocalOrAction(
                returnUrl, 
                nameof(MyCollection));
        }
    }
}
