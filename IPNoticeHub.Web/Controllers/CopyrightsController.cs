using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.DTOs;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;

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

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                publicId, 
                cancellationToken);
            
            if (dto is null) return NotFound();

            var viewModel = 
                CopyrightsMapping.MapDetailsDtoToCeaseDesistViewModel(dto, publicId);

            viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey(
                "CND-Copyright")!.BodyTemplate ?? string.Empty;

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

            var input = 
                CopyrightsMapping.MapCeaseDesistViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateCopyrightCeaseDesistAsync(
                input, 
                cancellationToken);

            return File(pdf, "application/pdf", 
                $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(CeaseDesistViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider
                    .GetTemplateByKey("CND-Copyright")?.BodyTemplate ?? string.Empty;

                var placeholders = 
                    CopyrightsMapping.MapCeaseDesistViewModelToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
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

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                viewModel.PublicId, 
                cancellationToken);
            
            if (dto != null) ApplyCopyrightCeaseDesistDetails(
                viewModel, 
                dto, 
                MergeStrategy.FillBlanks);

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey(
                    "CND-Copyright")?.BodyTemplate ?? string.Empty;

                var placeholders = 
                    CopyrightsMapping.MapCeaseDesistViewModelToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return View("CeaseDesistPreview", viewModel);
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel model)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            return View("CeaseDesistEdit", model);
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
                    CopyrightsMapping.MapCdViewModelToDocCreateDto(viewModel);

                await documentLibraryService.SaveDocumentAsync(userId, dto, cancellationToken);

                TempData["SuccessMessage"] =
                    "Your Cease & Desist letter was successfully saved to your library.";

                return RedirectToAction(nameof(CeaseDesistEdit), viewModel);
            }
 
            else if (command == "done")
            {
                return RedirectToAction("MyCollection", "Copyrights");
            }

            else
            {
                return View("CeaseDesistEdit", viewModel);
            }
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                publicId, 
                cancellationToken);

            if (dto is null) return NotFound();

            var viewModel = 
                CopyrightsMapping.MapDetailsDtoToDmcaViewModel(dto,publicId);

           viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey(
               "DMCA-General")?.BodyTemplate ?? viewModel.BodyTemplate;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(
            Guid publicId, 
            DMCAViewModel viewModel, 
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(viewModel);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                publicId, 
                cancellationToken);
            
            if (dto is null) return NotFound();

            ApplyCopyrightDMCADetails(viewModel, dto, MergeStrategy.OverwriteAll);

            var input = CopyrightsMapping.MapDmcaViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateCopyrightDMCAAsync(
                input, 
                cancellationToken);

            return File(pdf, "application/pdf",
                $"DMCA-{dto.Title}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(DMCAViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider
                    .GetTemplateByKey("DMCA-Copyright")?.BodyTemplate ?? string.Empty;

                var placeholders = 
                    CopyrightsMapping.MapDmcaViewModelToPlaceholders(viewModel);
                
                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
            }

            return View("DmcaPreview", viewModel);
        }

            [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
            public async Task<IActionResult> DmcaPreview(
                DMCAViewModel viewModel, 
                CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View("DMCA", viewModel);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId, 
                viewModel.PublicId, 
                cancellationToken);
            
            if (dto != null) ApplyCopyrightDMCADetails(
                viewModel, 
                dto, 
                MergeStrategy.FillBlanks);


            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || 
                viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey(
                    "DMCA-General")?.BodyTemplate ?? viewModel.BodyTemplate;

                var placeholders = 
                    CopyrightsMapping.MapDmcaViewModelToPlaceholders(viewModel);

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return RedirectToAction(nameof(DmcaPreview), viewModel);
        }        

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(DMCAViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            return View("DmcaEdit", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaEdit(
            DMCAViewModel viewModel, 
            string command,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid) return View("DmcaEdit", viewModel);

            if (command == "save")
            {
                var dto = 
                    CopyrightsMapping.MapDmcaViewModelToDocCreateDto(viewModel);

                await documentLibraryService.SaveDocumentAsync(userId, dto, cancellationToken);

                TempData["SuccessMessage"] =
                    "Your DMCA notice was successfully saved to your library.";

                return RedirectToAction(nameof(DmcaEdit), viewModel);
            }

            else if (command == "done")
            {
                return RedirectToAction("MyCollection", "Copyrights");
            }

            else
            {
                return View("DmcaEdit", viewModel);
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
                document.SourceType != DocumentSourceType.Copyright ||
                document.TemplateType != LetterTemplateType.CeaseAndDesist)
            {
                return NotFound();
            }

            var viewModel = LegalDocumentMapping.MapDocumentToCeaseDesistViewModel(document);

            return View("CeaseDesistEdit", viewModel);
        }

        [HttpGet]
        [Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> RecoverDmca(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var document = await documentLibraryService.GetSingleDocumentByIdAsync(
                documentId,
                userId,
                cancellationToken);

            if (document == null ||
                document.SourceType != DocumentSourceType.Copyright ||
                document.TemplateType != LetterTemplateType.Dmca)
            {
                return NotFound();
            }

            var viewModel = LegalDocumentMapping.MapDocumentToDmcaViewModel(document);

            return View("DmcaEdit", viewModel);
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
