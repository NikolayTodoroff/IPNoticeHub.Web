using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Web.Infrastructure;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.DTOs;

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

        [HttpGet,Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await tmSearchService.GetDetailsAsync(
                publicId, 
                cancellationToken);

            if (dto == null) return NotFound();

            bool isInCollection = await tmCollectionService.IsInCollectionAsync(
                userId, 
                dto.Id, 
                includeSoftDeleted: false, 
                cancellationToken);

            if (!isInCollection) return NotFound();

            var viewModel = TrademarksMapping.MapCeaseDesistViewModel(
                publicId,
                dto.Wordmark, 
                dto.RegistrationNumber!);

            viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey(
                "CND-Trademark")?.BodyTemplate ?? string.Empty;

            ViewData["ShowAdditionalFacts"] = true;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CeaseDesistViewModel viewModel, 
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var input = TrademarksMapping.
                MapCeaseDesistViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateTrademarkCeaseDesistAsync(
                input, 
                cancellationToken);

            return File(pdf, "application/pdf", 
                $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistPreview(CeaseDesistViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey(
                    "CND-Trademark")?.BodyTemplate ?? string.Empty;

                var placeholders = 
                    TrademarksMapping.MapCeaseDesistViewModellToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(
                    template, 
                    placeholders!);
            }

            return View("CeaseDesistPreview", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(
            CeaseDesistViewModel viewModel, 
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View("CeaseDesist", viewModel);
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await tmSearchService.GetDetailsAsync(
                viewModel.PublicId, 
                cancellationToken);

            if (dto != null) ApplyTrademarkCeaseDesistDetails(
                viewModel, 
                dto, 
                MergeStrategy.FillBlanks);    
                
            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey(
                    "CND-Trademark")?.BodyTemplate ?? string.Empty;

                var placeholders = 
                    TrademarksMapping.MapCeaseDesistViewModellToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(
                    template, 
                    placeholders);
            }

            return RedirectToAction(nameof(CeaseDesistPreview), viewModel);
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            return View("CeaseDesistEdit", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistEdit(
            CeaseDesistViewModel viewModel,
            string command, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid) return View("CeaseDesistEdit", viewModel);

            if (command == "save")
            {
                var dto = 
                    TrademarksMapping.MapCdViewModelToDocCreateDto(viewModel);

                await documentLibraryService.SaveDocumentAsync(
                    userId, 
                    dto,
                    cancellationToken);

                TempData["SuccessMessage"] = 
                    "Your Cease & Desist letter was successfully saved to your library.";

                return RedirectToAction(nameof(CeaseDesistEdit), viewModel);
            }

            else if (command == "done")
            {
                return RedirectToAction("MyCollection", "Trademarks");
            }

            else
            {
                return View("CeaseDesistEdit", viewModel);
            }
        }

        [HttpGet]
        [Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> RecoverCeaseDesist(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var document = await documentLibraryService.GetSingleDocumentByIdAsync(
                documentId,
                userId,
                cancellationToken);

            if (document == null ||
                document.SourceType != DocumentSourceType.Trademark ||
                document.TemplateType != LetterTemplateType.CeaseAndDesist)
            {
                return NotFound();
            }

            var viewModel = 
                LegalDocumentMapping.MapDocumentToCeaseDesistViewModel(document);

            return View("CeaseDesistEdit", viewModel);
        }
    }
}
