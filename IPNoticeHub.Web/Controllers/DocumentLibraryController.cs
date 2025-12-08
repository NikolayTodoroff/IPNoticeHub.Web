using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Web.Infrastructure.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPNoticeHub.Web.Infrastructure.Mappings;
namespace IPNoticeHub.Web.Controllers
{
    public class DocumentLibraryController : Controller
    {
        private readonly IDocumentLibraryService documentService;

        public DocumentLibraryController(
            IDocumentLibraryService service)
        {
            this.documentService = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DocumentSourceType? sourceType,
            LetterTemplateType? templateType,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var documents = await documentService.GetUserDocumentsAsync(
                userId,
                sourceType,
                templateType,
                cancellationToken);

            return View(documents);
        }

        [HttpGet]
        [Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Edit(
            int id, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var document = await documentService.GetSingleDocumentByIdAsync(
                id, 
                userId, 
                cancellationToken);
            
            if (document is null) return NotFound();

            if (document.TemplateType == LetterTemplateType.CeaseAndDesist)
            {
                var viewModel = LegalDocumentMapping.MapDocumentToCeaseDesistViewModel(document);
                //viewModel.Command = "update";          // hidden field or similar
                return View("CeaseDesistEdit", viewModel);
            }

            if (document.TemplateType == LetterTemplateType.Dmca)
            {
                var viewModel = LegalDocumentMapping.MapDocumentToDmcaViewModel(document);
                //vm.Command = "update";
                return View("DmcaEdit", viewModel);
            }

            return BadRequest("Unsupported template.");
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(
            int id,
            string newTitle,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                TempData["ErrorMessage"] = "Title cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            await documentService.RenameDocumentAsync(id, userId, newTitle, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await documentService.DeleteDocumentAsync(
                id, 
                userId, 
                cancellationToken);

            TempData["SuccessMessage"] = "Document deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(
            int id, 
            CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var restoredFile = await documentService.RestoreDocumentSnapshotAsync(
                id, 
                userId, 
                cancellationToken);

            if (restoredFile is null)
            {
                TempData["ErrorMessage"] = 
                    "Document not found or you do not have access to it.";

                return RedirectToAction(nameof(Index));
            }

            var (fileName, pdf) = restoredFile.Value;

            return File(pdf, "application/pdf", fileName);
        }
    }
}
