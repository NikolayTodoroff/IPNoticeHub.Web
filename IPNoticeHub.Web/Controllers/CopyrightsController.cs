using IPNoticeHub.Common;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Web.Controllers
{
    [Authorize(Policy = "HasUserId")]
    public sealed class CopyrightsController : Controller
    {
        private readonly ICopyrightService copyrightService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;
        public CopyrightsController(ICopyrightService service,IPdfService pdfService,ILetterTemplateProvider letterTemplateProvider)
        {
            this.copyrightService = service;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
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
            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(new CopyrightCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CopyrightCreateDTO createCopyrightDTO, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return View(createCopyrightDTO);
            }
         
            Guid publicId = await copyrightService.CreateAsync(userId, createCopyrightDTO, cancellationToken);

            TempData["SuccessMessage"] = CopyrightAddedMessage;

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

            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(editViewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost("Copyrights/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CopyrightEditViewModel editViewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return Forbid();
            }

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
                ModelState.AddModelError(string.Empty, CopyrightUpdatesErrorMessage);
                return View(editViewModel);
            }

            TempData["SuccessMessage"] = CopyrightUpdatesMessage;

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

            TempData["SuccessMessage"] = CopyrightRemovedMessage;
            return RedirectToLocal(returnUrl)
                ?? RedirectToAction(nameof(MyCollection));
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            var viewModel = new DMCAViewModel
            {
                PublicId = publicId,
                WorkTitle = copyrightDetailsDTO.Title,
                RegistrationNumber = copyrightDetailsDTO.RegistrationNumber,
                YearOfCreation = copyrightDetailsDTO.YearOfCreation,
                DateOfPublication = copyrightDetailsDTO.DateOfPublication,
                NationOfFirstPublication = copyrightDetailsDTO.NationOfFirstPublication
            };

           viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate 
                ?? viewModel.BodyTemplate;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> Dmca(Guid publicId, DMCAViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var details = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (details is null)
            {
                return NotFound();
            }

            var input = new DMCAInput(
                SenderName: viewModel.SenderName,
                SenderEmail: viewModel.SenderEmail,
                SenderAddress: viewModel.SenderAddress,
                RecipientName: viewModel.RecipientName,
                RecipientEmail: viewModel.RecipientEmail ?? string.Empty,
                RecipientAddress: viewModel.RecipientAddress ?? string.Empty,
                Date: DateTime.UtcNow,
                WorkTitle: details.Title,
                RegistrationNumber: details.RegistrationNumber,
                YearOfCreation: viewModel.YearOfCreation,
                DateOfPublication: viewModel.DateOfPublication,
                NationOfFirstPublication: viewModel.NationOfFirstPublication,
                InfringingUrl: viewModel.InfringingUrl,
                GoodFaithStatement: viewModel.GoodFaithStatement,
                BodyTemplate: viewModel.BodyTemplate
            );

            var pdf = await pdfService.GenerateCopyrightDMCAAsync(input, cancellationToken);
            return File(pdf, "application/pdf",
                $"DMCA-{details.Title}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(DMCAViewModel viewModel, CancellationToken ct = default)
        {
            if (!User.TryGetUserId(out var _)) return Forbid();

            // Load default template
            var template = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ?? viewModel.BodyTemplate;

            var placeholders = new Dictionary<string, string>
            {
                ["Date"] = DateTime.UtcNow.ToString(DateTimeFormat),
                ["RecipientName"] = viewModel.RecipientName ?? "",
                ["RecipientAddress"] = viewModel.RecipientAddress ?? "",
                ["RecipientEmail"] = viewModel.RecipientEmail ?? "",
                ["SenderName"] = viewModel.SenderName ?? "",
                ["SenderAddress"] = viewModel.SenderAddress ?? "",
                ["SenderEmail"] = viewModel.SenderEmail ?? "",
                ["WorkTitle"] = viewModel.WorkTitle ?? "",
                ["RegistrationNumber"] = viewModel.RegistrationNumber ?? "",
                ["InfringingUrl"] = viewModel.InfringingUrl ?? "",
                ["YearOfCreation"] = viewModel.YearOfCreation?.ToString() ?? "",
                ["DateOfPublication"] = viewModel.DateOfPublication?.ToString(DateTimeFormat) ?? "",
                ["NationOfFirstPublication"] = viewModel.NationOfFirstPublication ?? "",
                ["GoodFaithStatement"] = viewModel.GoodFaithStatement ?? ""
            };

            viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            return View("DMCAPreview", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(DMCAViewModel model)
        {
            return View("DMCAEdit", model);
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult>CeaseDesist(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = copyrightDetailsDTO.Title,
                RegistrationNumber = copyrightDetailsDTO.RegistrationNumber
                // Sender/Recipient left blank for user to fill
            };

            var presets = letterTemplateProvider.GetLetterTemplatePresets(LetterTemplateType.CeaseDesist);

            var template = letterTemplateProvider.GetTemplateByKey("CND-Copyright")!;
            viewModel.BodyTemplate = template.BodyTemplate;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(Guid publicId, CeaseDesistViewModel viewModel, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var input = new CeaseDesistInput(
              SenderName: viewModel.SenderName,
              SenderAddress: viewModel.SenderAddress,
              RecipientName: viewModel.RecipientName,
              RecipientAddress: viewModel.RecipientAddress,
              Date: DateTime.UtcNow,
              WorkTitle: viewModel.WorkTitle,
              RegistrationNumber: viewModel.RegistrationNumber ?? string.Empty,
              AdditionalFacts: viewModel.AdditionalFacts,
              BodyTemplate: viewModel.BodyTemplate
            );

            var pdf = await pdfService.GenerateCopyrightCeaseDesistAsync(input, ct);
            return File(pdf, "application/pdf", $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        /// <summary>
        /// Redirects to a local URL if the provided returnUrl is valid and local.
        /// Returns null if the returnUrl is null, empty, or not a local URL.
        /// </summary>
        private IActionResult? RedirectToLocal(string? returnUrl)
        {

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))

                return Redirect(returnUrl);

            return null;

        }

        /// <summary>
        /// Maps the stored type of work string to a CopyrightWorkType enum value and an optional "other" text.
        /// If the stored value matches a predefined enum value, it returns the enum value and null for "other".
        /// Otherwise, it returns CopyrightWorkType.Other and the stored value as "other".
        /// </summary>
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

        /// <summary>
        /// Replaces placeholders in the given template string with corresponding values from the provided dictionary.
        /// If a key is not found in the dictionary, the placeholder remains unchanged.
        /// </summary>
        private static string ReplaceTemplate(string template, IDictionary<string, string> vars)
        {
            return System.Text.RegularExpressions.Regex.Replace(template ?? string.Empty, "{{\\s*(\\w+)\\s*}}", m =>
            {
                var key = m.Groups[1].Value;
                return vars.TryGetValue(key, out var val) ? (val ?? string.Empty) : m.Value;
            });
        }
    }
}
