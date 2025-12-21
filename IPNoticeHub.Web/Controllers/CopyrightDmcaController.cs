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
using static IPNoticeHub.Web.WebHelpers.ApplyEntityDetails;
using static IPNoticeHub.Shared.Constants.StatusMessages.PreviewPageMessages;
using static IPNoticeHub.Shared.Constants.StatusMessages.EditPageMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public class CopyrightDmcaController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfLetterService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        private readonly IDocumentLibraryService documentLibraryService;
        private readonly ITemplateTokenReplacer templateReplacer;
        private readonly IUserInputDraftStore draftStore;

        public CopyrightDmcaController(
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
            DmcaViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(viewModel);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(
                userId,
                publicId,
                cancellationToken);

            if (dto is null) return NotFound();

            ApplyCopyrightDMCADetails(
                viewModel, 
                dto, 
                MergeStrategy.OverwriteAll);

            var input = 
                CopyrightsMapping.MapDmcaViewModelToInput(viewModel);

            var pdf = await pdfService.GenerateFromInputAsync(input, cancellationToken);

            return File(pdf, "application/pdf",
                $"DMCA-{dto.Title}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(
            Guid publicId,
            Guid? draftId,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (draftId is not Guid id)
            {
                TempData["PreviewInfo"] = NoDataMessage;
                return RedirectToAction(nameof(Dmca), new { publicId });
            }

            var draftDto =
                await draftStore.GetAsync<DmcaDraftDto>(
                    userId: userId,
                    draftId: id,
                    keySpace: CopyrightDmcaKeySpace,
                    cancellationToken: cancellationToken);

            if (draftDto is null)
            {
                TempData["PreviewInfo"] = SessionExpiredMessage;
                return RedirectToAction(nameof(Dmca), new { publicId });
            }

            var copyrightDto =
                await copyrightService.GetDetailsAsync(
                userId,
                publicId,
                cancellationToken);

            if (copyrightDto is null)
            {
                TempData["PreviewInfo"] = CopyrightDetailsMissingMessage;
                return RedirectToAction(nameof(Dmca), new { publicId });
            }

            var viewModel =
                CopyrightsMapping.MapDetailsDtoToDmcaViewModel(
                copyrightDto,
                publicId);

            UserInputDraftMapping.MapDraftDtoToDmcaViewModel(viewModel, draftDto);

            var template =
                letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ??
                string.Empty;

            var placeholders =
                UserInputDraftMapping.MapDmcaViewModelToPlaceholders(viewModel);

            viewModel.BodyTemplate = templateReplacer.ReplaceTemplate(template, placeholders);

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(
            DmcaViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View("DMCA", viewModel);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            viewModel.BodyTemplate = string.Empty;

            var draftDto =
                UserInputDraftMapping.MapDmcaViewModelDraftDto(viewModel);

            var draftId = await draftStore.SaveAsync(
                userId: userId,
                keySpace: CopyrightDmcaKeySpace,
                payload: draftDto,
                ttl: InputTtl,
                cancellationToken: cancellationToken);


            return RedirectToAction(
                nameof(DmcaPreview),
                new { publicId = viewModel.PublicId, draftId });
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(DmcaViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            return View("DmcaEdit", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaEdit(
            DmcaViewModel viewModel,
            string command,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid) return View("DmcaEdit", viewModel);

            if (command == "save")
            {
                var dto =
                    CopyrightsMapping.MapDmcaViewModelToDocCreateDto(viewModel);

                await documentLibraryService.SaveDocumentAsync(
                    userId, 
                    dto, 
                    cancellationToken);

                TempData["SuccessMessage"] = DmcaNoticeSavedMessage;

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

            var viewModel = 
                LegalDocumentMapping.MapDocumentToDmcaViewModel(document);

            return View("DmcaEdit", viewModel);
        }
    }
}
