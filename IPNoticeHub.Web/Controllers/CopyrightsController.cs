using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Extensions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Web.Models.Copyrights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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

            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(Details), new { id = publicId });
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet("Copyrights/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);
            if (copyrightDetailsDTO is null) return NotFound();

            var (workType, otherText) = TypeOfWorkMapper(copyrightDetailsDTO.TypeOfWork);

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = copyrightDetailsDTO.PublicId,
                RegistrationNumber = copyrightDetailsDTO.RegistrationNumber,
                WorkType = workType,
                OtherWorkType = otherText,
                Title = copyrightDetailsDTO.Title,
                YearOfCreation = copyrightDetailsDTO.YearOfCreation,
                DateOfPublication = copyrightDetailsDTO.DateOfPublication,
                Owner = copyrightDetailsDTO.Owner,
                NationOfFirstPublication = copyrightDetailsDTO.NationOfFirstPublication
            };

            return View(editViewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost("Copyrights/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CopyrightEditViewModel editViewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            // Conditional validation: OtherWorkType required when WorkType == Other
            if (editViewModel.WorkType == CopyrightWorkType.Other && string.IsNullOrWhiteSpace(editViewModel.OtherWorkType))
            {
                ModelState.AddModelError(nameof(CopyrightEditViewModel.OtherWorkType), "Please specify the work type.");
            }

            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }

            var copyrightEditDTO = new CopyrightEditDTO
            {
                RegistrationNumber = editViewModel.RegistrationNumber,
                WorkType = editViewModel.WorkType,
                OtherWorkType = editViewModel.OtherWorkType,
                Title = editViewModel.Title,
                YearOfCreation = editViewModel.YearOfCreation,
                DateOfPublication = editViewModel.DateOfPublication,
                Owner = editViewModel.Owner,
                NationOfFirstPublication = editViewModel.NationOfFirstPublication
            };

            var editedSuccessfully = await copyrightService.EditAsync(userId, id, copyrightEditDTO, cancellationToken);

            if (!editedSuccessfully)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Ensure you have this item in your collection and the registration number is unique.");
                return View(editViewModel);
            }

            TempData["StatusMessage"] = "Copyright updated.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }         

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            CopyrightDetailsDTO? model = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);

            if (model is null) return NotFound();

            return View(model);
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

        private static (CopyrightWorkType workType, string? other) TypeOfWorkMapper(string stored)
        {
            if (Enum.TryParse<CopyrightWorkType>(stored, ignoreCase: true, out var parsedWorkType) &&
                parsedWorkType != CopyrightWorkType.Other)
            {
                return (parsedWorkType, null);
            }

            var other = string.IsNullOrWhiteSpace(stored) ? null : stored;
            return (CopyrightWorkType.Other, other);
        }
    }
}
