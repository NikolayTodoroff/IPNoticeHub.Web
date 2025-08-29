using IPNoticeHub.Common.EnumConstants; 
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IPNoticeHub.Common.EntityValidationConstants.PagingConstants;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize]
    public sealed class CopyrightsController : Controller
    {
        private readonly ICopyrightService service;

        public CopyrightsController(ICopyrightService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, int resultsPerPage = DefaultPageSize, CancellationToken cancellationToken = default)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            var model = await service.GetUserCollectionAsync(
                userId, sortBy, currentPage, resultsPerPage, cancellationToken);

            ViewBag.SortBy = sortBy;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            CopyrightDetailsDTO? model = await service.GetDetailsAsync(userId, id, cancellationToken);
            
            if (model is null) return NotFound();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CopyrightCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CopyrightCreateDTO dto, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            string? userId = GetUserId();

            if (userId is null) return Challenge();

            if (!ModelState.IsValid) return View(dto);

            Guid publicId = await service.CreateAsync(userId, dto, cancellationToken);

            TempData["StatusMessage"] = "Copyright added to your collection.";

            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(Details), new { id = publicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            string? userId = GetUserId();
            if (userId is null) return Challenge();

            await service.RemoveAsync(userId, id, cancellationToken);

            TempData["StatusMessage"] = "Copyright removed from your collection.";
            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(MyCollection));
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}
