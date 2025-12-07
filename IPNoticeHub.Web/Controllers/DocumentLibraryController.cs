using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPNoticeHub.Web.Controllers
{
    public class DocumentLibraryController : Controller
    {
        private readonly IDocumentLibraryService documentService;   

        public DocumentLibraryController(IDocumentLibraryService service)
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

            TempData["SuccessMessage"] = "Document renamed successfully.";
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
    }
}
