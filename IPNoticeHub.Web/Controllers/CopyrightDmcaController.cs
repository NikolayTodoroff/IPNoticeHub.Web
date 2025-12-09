using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.PdfGeneration.Abstractions;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class CopyrightDmcaController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;

        public CopyrightDmcaController(
            ICopyrightService copyrightService,
            IPdfService pdfService,
            ILetterTemplateProvider letterTemplateProvider,
            IDocumentLibraryService documentLibraryService)
        {
            this.copyrightService = copyrightService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
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
                CopyrightsMapping.MapDetailsDtoToDmcaViewModel(dto, publicId);

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

            else return View("DmcaEdit", viewModel);
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
    }
}
