using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
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

            var documents = 
                await documentService.GetUserDocumentsAsync(
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

            var document = 
                await documentService.GetSingleDocumentByIdAsync(
                id, 
                userId, 
                cancellationToken);
            
            if (document is null) return NotFound();

            if (document.SourceType == DocumentSourceType.Trademark)
            {
                return RedirectToAction(
                    actionName: "RecoverCeaseDesist",
                    controllerName: "TrademarkCad",
                    routeValues: new { documentId = document.LegalDocumentId });
            }

            if (document.SourceType == DocumentSourceType.Copyright &&
                document.TemplateType == LetterTemplateType.CeaseAndDesist)
            {
                return RedirectToAction(
                    actionName: "RecoverCeaseDesist",
                    controllerName: "CopyrightCad",
                    routeValues: new { documentId = document.LegalDocumentId });
            }

            if (document.SourceType == DocumentSourceType.Copyright &&
                document.TemplateType == LetterTemplateType.Dmca)
            {
                return RedirectToAction(
                    actionName: "RecoverDmca",
                    controllerName: "CopyrightDmca",
                    routeValues: new { documentId = document.LegalDocumentId });
            }

            return BadRequest("Unsupported document type.");
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

            await documentService.RenameDocumentAsync(
                id, 
                userId, 
                newTitle, 
                cancellationToken);

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

            var restoredFile = 
                await documentService.RestoreDocumentSnapshotAsync(
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
