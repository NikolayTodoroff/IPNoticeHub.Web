using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Web.Models.Copyrights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Shared.Constants.StatusMessages.CopyrightStatusMessages;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Services.PdfGeneration.Abstractions;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public sealed class CopyrightsController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;

        public CopyrightsController(
            ICopyrightService service,
            IPdfService pdfService,
            ILetterTemplateProvider letterTemplateProvider, 
            IDocumentLibraryService documentLibraryService)
        {
            this.copyrightService = service;
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

            var dto = await copyrightService.GetUserCollectionAsync(
                userId, 
                sortBy, 
                currentPage, 
                resultsPerPage, 
                cancellationToken);

            var viewmodel = 
                CopyrightsMapping.MapCollectionDtoToViewModel(dto);

            ViewBag.SortBy = sortBy;

            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(new CopyrightCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CopyrightCreateViewModel viewModel, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return View(viewModel);
            }

            var dto = CopyrightsMapping.MapNewCopyrightViewModelToDto(viewModel);

            Guid publicId = await copyrightService.CreateAsync(
                userId, 
                dto, 
                cancellationToken);

            TempData["SuccessMessage"] = CopyrightAddedMessage;

            return RedirectToLocal(returnUrl) ?? 
                RedirectToAction(nameof(Details), new { id = publicId });
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet("Copyrights/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(
            Guid id, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                id, 
                cancellationToken);

            if (dto is null) return NotFound();

            var (workType, otherText) = TypeOfWorkMapper(dto.TypeOfWork);

            var viewModel = 
                CopyrightsMapping.MapDetailsDtoToEditViewModel(
                    dto,
                    workType,
                    otherText);

            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(viewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost("Copyrights/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id, 
            CopyrightEditViewModel viewModel, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return Forbid();
            }

            if (viewModel.WorkType == CopyrightWorkType.Other && 
                string.IsNullOrWhiteSpace(viewModel.OtherWorkType))
            {
                ModelState.AddModelError(
                    nameof(CopyrightEditViewModel.OtherWorkType), 
                    "Please specify the work type.");
            }

            if (!ModelState.IsValid) return View(viewModel);

            var dto = CopyrightsMapping.MapEditViewModelToDto(viewModel);

            bool editedSuccessfully = await copyrightService.EditAsync(
                userId, 
                id, 
                dto, 
                cancellationToken);

            if (!editedSuccessfully)
            {
                ModelState.AddModelError(string.Empty, CopyrightUpdatesErrorMessage);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = CopyrightUpdatesMessage;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }         

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            Guid id, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                id, 
                cancellationToken);
            
            if (dto is null) return NotFound();

            var viewModel = 
                CopyrightsMapping.MapDetailsDtoToViewModel(dto);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            Guid id, 
            string? returnUrl = null, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await copyrightService.RemoveAsync(userId, id, cancellationToken);

            TempData["SuccessMessage"] = CopyrightRemovedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return null;
        }

        private static (CopyrightWorkType workType, string? other) TypeOfWorkMapper(string stored)
        {
            if (Enum.TryParse<CopyrightWorkType>(
                stored, ignoreCase: true, out var parsedWorkType) &&
                parsedWorkType != CopyrightWorkType.Other)
            {
                return (parsedWorkType, null);
            }

            var other = string.IsNullOrWhiteSpace(stored) ? null : stored;
            return (CopyrightWorkType.Other, other);
        }
    }
}
