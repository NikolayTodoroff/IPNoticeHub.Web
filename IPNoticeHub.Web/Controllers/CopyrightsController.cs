using IPNoticeHub.Common.EnumConstants; 
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using IPNoticeHub.Common.Extensions;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
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
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var model = await service.GetUserCollectionAsync(
                userId, sortBy, currentPage, resultsPerPage, cancellationToken);

            ViewBag.SortBy = sortBy;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            User.TryGetUserId(out var userId);

            CopyrightDetailsDTO? model = await service.GetDetailsAsync(userId!, id, cancellationToken);
            
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

            Guid publicId = await service.CreateAsync(userId, dto, cancellationToken);

            TempData["StatusMessage"] = CopyrightAddedMessage;

            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(Details), new { id = publicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await service.RemoveAsync(userId, id, cancellationToken);

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
