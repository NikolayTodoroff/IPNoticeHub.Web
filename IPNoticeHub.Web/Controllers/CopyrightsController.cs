using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;
using IPNoticeHub.Web.Infrastructure.Mappings;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public sealed class CopyrightsController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        public CopyrightsController(ICopyrightService service,IPdfService pdfService,ILetterTemplateProvider letterTemplateProvider)
        {
            this.copyrightService = service;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
        }


        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, int resultsPerPage = DefaultPageSize, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetUserCollectionAsync(
                userId, sortBy, currentPage, resultsPerPage, cancellationToken);

            var viewmodel = CopyrightsMapping.MapCollectionDtoToViewModel(dto);

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
        public async Task<IActionResult> Create(CopyrightCreateViewModel viewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return View(viewModel);
            }

            var dto = CopyrightsMapping.MapNewCopyrightViewModelToDto(viewModel);

            Guid publicId = await copyrightService.CreateAsync(userId, dto, cancellationToken);

            TempData["SuccessMessage"] = CopyrightAddedMessage;

            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(Details), new { id = publicId });
        }


        [Authorize(Policy = "HasUserId")]
        [HttpGet("Copyrights/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);
            if (dto is null) return NotFound();

            var (workType, otherText) = TypeOfWorkMapper(dto.TypeOfWork);

            var viewModel = CopyrightsMapping.MapDetailsDtoToEditViewModel(dto,workType,otherText);

            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(viewModel);
        }


        [Authorize(Policy = "HasUserId")]
        [HttpPost("Copyrights/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CopyrightEditViewModel viewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return Forbid();
            }

            // Conditional validation: OtherWorkType required when WorkType == Other
            if (viewModel.WorkType == CopyrightWorkType.Other && string.IsNullOrWhiteSpace(viewModel.OtherWorkType))
            {
                ModelState.AddModelError(nameof(CopyrightEditViewModel.OtherWorkType), "Please specify the work type.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var dto = CopyrightsMapping.MapEditViewModelToDto(viewModel);

            var editedSuccessfully = await copyrightService.EditAsync(userId, id, dto, cancellationToken);

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
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);

            if (dto is null) return NotFound();

            var viewModel = CopyrightsMapping.MapDetailsDtoToViewModel(dto);

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await copyrightService.RemoveAsync(userId, id, cancellationToken);

            TempData["SuccessMessage"] = CopyrightRemovedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var dto = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (dto is null)
            {
                return NotFound();
            }

            var viewModel = CopyrightsMapping.MapDetailsDtoToDmcaViewModel(dto,publicId);

           viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate 
                ?? viewModel.BodyTemplate;

            return View(viewModel);
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(Guid publicId, DMCAViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightDMCADetails(viewModel, copyrightDetailsDTO, MergeStrategy.OverwriteAll);

            var input = CopyrightsMapping.MapDmcaViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateCopyrightDMCAAsync(input, cancellationToken);

            return File(pdf, "application/pdf",
                $"DMCA-{copyrightDetailsDTO.Title}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var dto = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (dto is null) return NotFound();

            var viewModel = new DMCAViewModel { PublicId = publicId };

            ApplyCopyrightDMCADetails(viewModel, dto, MergeStrategy.FillBlanks);

            var template = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ?? string.Empty;

            var placeholders = CopyrightsMapping.MapDmcaViewModelToPlaceholders(viewModel);

            viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
            return View("DMCAPreview", viewModel);
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(DMCAViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("DMCA", viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, viewModel.PublicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightDMCADetails(viewModel, copyrightDetailsDTO, MergeStrategy.FillBlanks);

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ?? viewModel.BodyTemplate;

                var placeholders = CopyrightsMapping.MapDmcaViewModelToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return View("DMCAPreview", viewModel);
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(Guid publicId, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? 
                returnUrl : Url.Action(nameof(DmcaPreview), new { publicId })!;

            return View("DMCAEdit", new DMCAViewModel { PublicId = publicId });
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(DMCAViewModel model)
        {
            return View("DMCAEdit", model);
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult>CeaseDesist(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var dto = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (dto is null)
            {
                return NotFound();
            }

            var viewModel = CopyrightsMapping.MapDetailsDtoToCeaseDesistViewModel(dto, publicId);

            viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey("CND-Copyright")!.BodyTemplate ?? string.Empty;
            ViewData["ShowAdditionalFacts"] = true;

            return View(viewModel);
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(Guid publicId, CeaseDesistViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var input = CopyrightsMapping.MapCeaseDesistViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateCopyrightCeaseDesistAsync(input, cancellationToken);
            return File(pdf, "application/pdf", $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(Guid publicId, bool reset = false, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            var viewModel = new CeaseDesistViewModel { PublicId = publicId };
            var template = letterTemplateProvider.GetTemplateByKey(key: "CND-Copyright")?.BodyTemplate ?? string.Empty;

            var placeholders = CopyrightsMapping.MapCeaseDesistViewModelToPlaceholders(viewModel);

            viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
            return View(viewName: "CeaseDesistPreview", viewModel);
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(CeaseDesistViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("CeaseDesist", viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, viewModel.PublicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightCeaseDesistDetails(viewModel, copyrightDetailsDTO, MergeStrategy.FillBlanks);

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey("CND-Copyright")?.BodyTemplate ?? string.Empty;

                var placeholders = CopyrightsMapping.MapCeaseDesistViewModelToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return View("CeaseDesistPreview", viewModel);
        }


        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(Guid publicId, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? 
                returnUrl : Url.Action(nameof(CeaseDesistPreview), new { publicId })!;

            return View("CeaseDesistEdit", new CeaseDesistViewModel { PublicId = publicId });
        }


        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel model)
        {
            return View("CeaseDesistEdit", model);
        }

        /// <summary>
        /// Redirects to a local URL if the provided returnUrl is valid and local.
        /// Returns null if the returnUrl is null, empty, or not a local URL.
        /// </summary>
        private IActionResult? RedirectToLocal(string? returnUrl)
        {

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))

                return Redirect(returnUrl);

            return null;

        }

        /// <summary>
        /// Maps the stored type of work string to a CopyrightWorkType enum value and an optional "other" text.
        /// If the stored value matches a predefined enum value, it returns the enum value and null for "other".
        /// Otherwise, it returns CopyrightWorkType.Other and the stored value as "other".
        /// </summary>
        private static (CopyrightWorkType workType, string? other) TypeOfWorkMapper(string stored)
        {
            if (Enum.TryParse<CopyrightWorkType>(stored, ignoreCase: true, out var parsedWorkType) &&
                parsedWorkType != CopyrightWorkType.Other)
            {
                return (parsedWorkType, null);
            }

            var other = string.IsNullOrWhiteSpace(stored) ? null : stored;
            return (CopyrightWorkType.Other, other);
        } 
    }
}
