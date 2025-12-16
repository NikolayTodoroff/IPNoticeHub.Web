using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.WebHelpers.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Web.WebHelpers.ApplyEntityDetails;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class TrademarkCadController : Controller
    {
        private readonly ITrademarkSearchService trademarkSearchService;
        private readonly ITrademarkCollectionService trademarkCollectionService;
        private readonly IPdfLetterService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;
        private readonly ITemplateTokenReplacer templateReplacer;

        public TrademarkCadController(
            ITrademarkSearchService trademarkSearchService,
            ITrademarkCollectionService trademarkCollectionService,
            IPdfLetterService pdfService,
            ILetterTemplateProvider letterTemplateProvider,
            IDocumentLibraryService documentLibraryService,
            ITemplateTokenReplacer templateReplacer)
        {
            this.trademarkSearchService = trademarkSearchService;
            this.trademarkCollectionService = trademarkCollectionService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
            this.templateReplacer = templateReplacer;
        }

        [HttpGet]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = 
                await trademarkSearchService.GetDetailsAsync(
                publicId,
                cancellationToken);

            if (dto == null) return NotFound();

            bool isInCollection = 
                await trademarkCollectionService.IsInCollectionAsync(
                userId,
                dto.Id,
                includeSoftDeleted: false,
                cancellationToken);

            if (!isInCollection) return NotFound();

            var viewModel = 
                TrademarksMapping.MapCeaseDesistViewModel(
                publicId,
                dto.Wordmark,
                dto.RegistrationNumber!);

            viewModel.BodyTemplate = 
                letterTemplateProvider.GetTemplateByKey(
                "CND-Trademark")?.BodyTemplate ?? string.Empty;

            ViewData["ShowAdditionalFacts"] = true;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CeaseDesistViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var input = 
                TrademarksMapping.MapCeaseDesistViewModelInput(viewModel);

            var pdf = await pdfService.GenerateFromInputAsync(input, cancellationToken);

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

                viewModel.BodyTemplate =
                    templateReplacer.ReplaceTemplate(template, placeholders);
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

                await documentLibraryService.SaveDocumentAsync(userId,dto,cancellationToken);

                TempData["SuccessMessage"] =
                    "Your Cease & Desist letter was successfully saved to your library.";

                return RedirectToAction(nameof(CeaseDesistEdit), viewModel);
            }

            else if (command == "done")
            {
                return RedirectToAction("MyCollection", "Trademarks");
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
