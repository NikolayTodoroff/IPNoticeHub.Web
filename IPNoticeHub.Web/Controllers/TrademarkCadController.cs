using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.PdfGeneration.Abstractions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Web.Infrastructure.Mappings;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class TrademarkCadController : Controller
    {
        private readonly ITrademarkSearchService trademarkSearchService;
        private readonly ITrademarkCollectionService trademarkCollectionService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;

        public TrademarkCadController(
            ITrademarkSearchService trademarkSearchService,
            ITrademarkCollectionService trademarkCollectionService,
            IPdfService pdfService,
            ILetterTemplateProvider letterTemplateProvider,
            IDocumentLibraryService documentLibraryService)
        {
            this.trademarkSearchService = trademarkSearchService;
            this.trademarkCollectionService = trademarkCollectionService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await trademarkSearchService.GetDetailsAsync(
                publicId,
                cancellationToken);

            if (dto == null) return NotFound();

            bool isInCollection = await trademarkCollectionService.IsInCollectionAsync(
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

            var dto = await trademarkSearchService.GetDetailsAsync(
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
