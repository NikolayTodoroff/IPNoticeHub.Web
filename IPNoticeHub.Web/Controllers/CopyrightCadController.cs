using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationService.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.WebHelpers.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Web.WebHelpers.ApplyEntityDetails;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class CopyrightCadController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;
        private readonly ITemplateTokenReplacer templateReplacer;

        public CopyrightCadController(
            ICopyrightService copyrightService,
            IPdfService pdfService,
            ILetterTemplateProvider letterTemplateProvider,
            IDocumentLibraryService documentLibraryService, 
            ITemplateTokenReplacer templateReplacer)
        {
            this.copyrightService = copyrightService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
            this.templateReplacer = templateReplacer;
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = 
                await copyrightService.GetDetailsAsync(
                userId,
                publicId,
                cancellationToken);

            if (dto is null) return NotFound();

            var viewModel =
                CopyrightsMapping.MapDetailsDtoToCeaseDesistViewModel(
                    dto, 
                    publicId);

            viewModel.BodyTemplate = 
                letterTemplateProvider.GetTemplateByKey(
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

                viewModel.BodyTemplate = 
                    templateReplacer.ReplaceTemplate(template, placeholders);
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

                viewModel.BodyTemplate =
                    templateReplacer.ReplaceTemplate(template, placeholders);
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
                return RedirectToAction("MyCollection", "Copyrights");
            }

            else return View("CeaseDesistEdit", viewModel);
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

            var viewModel = 
                LegalDocumentMapping.MapDocumentToCeaseDesistViewModel(document);

            return View("CeaseDesistEdit", viewModel);
        }
    }
}
