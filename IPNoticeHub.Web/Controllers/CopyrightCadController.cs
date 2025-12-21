using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.WebHelpers.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Shared.Constants.InputDraftConstants.UserInputDraftOptions;
using static IPNoticeHub.Shared.Constants.StatusMessages.PreviewPageMessages;
using static IPNoticeHub.Shared.Constants.StatusMessages.EditPageMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class CopyrightCadController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfLetterService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;
        private readonly ITemplateTokenReplacer templateReplacer;
        private readonly IUserInputDraftStore draftStore;

        public CopyrightCadController(
            ICopyrightService copyrightService,
            IPdfLetterService pdfService,
            ILetterTemplateProvider letterTemplateProvider,
            IDocumentLibraryService documentLibraryService, 
            ITemplateTokenReplacer templateReplacer, 
            IUserInputDraftStore draftStore)
        {
            this.copyrightService = copyrightService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
            this.documentLibraryService = documentLibraryService;
            this.templateReplacer = templateReplacer;
            this.draftStore = draftStore;
        }

        [HttpGet]
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CeaseDesist(
            Guid publicId,
            CeaseDesistViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var input =
                CopyrightsMapping.MapCeaseDesistViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateFromInputAsync(input, cancellationToken);

            return File(pdf, "application/pdf",
                $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> CeaseDesistPreview(
            Guid publicId, 
            Guid? draftId, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (draftId is not Guid id)
            {
                TempData["PreviewInfo"] = NoDataMessage;
                return RedirectToAction(nameof(CeaseDesist), new { publicId });
            }

            var draftDto = 
                await draftStore.GetAsync<CeaseDesistDraftDto>(
                    userId: userId,
                    draftId: id,
                    keySpace: CopyrightCadKeySpace,
                    cancellationToken: cancellationToken);

            if (draftDto is null)
            {
                TempData["PreviewInfo"] = SessionExpiredMessage;
                return RedirectToAction(nameof(CeaseDesist), new { publicId });
            }

            var copyrightDto = 
                await copyrightService.GetDetailsAsync(
                userId, 
                publicId, 
                cancellationToken);
            
            if (copyrightDto is null)
            {
                TempData["PreviewInfo"] = CopyrightDetailsMissingMessage;
                return RedirectToAction(nameof(CeaseDesist), new { publicId });
            }

            var viewModel = 
                CopyrightsMapping.MapDetailsDtoToCeaseDesistViewModel(
                copyrightDto, 
                publicId);

            UserInputDraftMapping.MapDraftDtoToCeaseDesistViewModel(viewModel, draftDto);

            var template = 
                letterTemplateProvider.GetTemplateByKey("CND-Copyright")?.BodyTemplate ?? 
                string.Empty;

            var placeholders = 
                UserInputDraftMapping.MapCeaseDesistViewModelToPlaceholders(viewModel);

            viewModel.BodyTemplate = templateReplacer.ReplaceTemplate(template, placeholders);

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CeaseDesistPreview(
            CeaseDesistViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View("CeaseDesist", viewModel);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            viewModel.BodyTemplate = string.Empty;

            var draftDto = 
                UserInputDraftMapping.MapCeaseDesistViewModelDraftDto(viewModel);

            var draftId = await draftStore.SaveAsync(
                userId: userId,
                keySpace: CopyrightCadKeySpace,
                payload: draftDto,
                ttl: InputTtl,                        
                cancellationToken: cancellationToken);

            return RedirectToAction(
                nameof(CeaseDesistPreview), 
                new { publicId = viewModel.PublicId, draftId });
        }

        [HttpGet]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel model)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            return View("CeaseDesistEdit", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
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

                TempData["SuccessMessage"] = CeaseDesistSavedMessage;

                return RedirectToAction(nameof(CeaseDesistEdit), viewModel);
            }

            else if (command == "done")
            {
                return RedirectToAction("MyCollection", "Copyrights");
            }

            else return View("CeaseDesistEdit", viewModel);
        }

        [HttpGet]
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
