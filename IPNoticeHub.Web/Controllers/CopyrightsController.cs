using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Extensions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public sealed class CopyrightsController : Controller
    {
        private readonly ICopyrightService copyrightService;

        public CopyrightsController(ICopyrightService service)
        {
            this.copyrightService = service;
        }

        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
            int currentPage = DefaultPage, int resultsPerPage = DefaultPageSize, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var model = await copyrightService.GetUserCollectionAsync(
                userId, sortBy, currentPage, resultsPerPage, cancellationToken);

            ViewBag.SortBy = sortBy;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            CopyrightDetailsDTO? model = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);
            
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
            if (!ModelState.IsValid) return View(dto);

            if (!User.TryGetUserId(out var userId)) return Forbid();

            Guid publicId = await copyrightService.CreateAsync(userId, dto, cancellationToken);

            TempData["StatusMessage"] = CopyrightAddedMessage;

            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(Details), new { id = publicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await copyrightService.RemoveAsync(userId, id, cancellationToken);

            TempData["StatusMessage"] = CopyrightRemovedMessage;
            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(MyCollection));
        }       

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return null;
        }
    }
}
