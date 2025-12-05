using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Web.Infrastructure;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;

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
            return View(new CopyrightCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CopyrightCreateViewModel viewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return View(viewModel);
            }

            var dto = new CopyrightCreateDto
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                WorkType = viewModel.WorkType,
                OtherWorkType = viewModel.OtherWorkType,
                Title = viewModel.Title,
                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                Owner = viewModel.Owner,
                NationOfFirstPublication = viewModel.NationOfFirstPublication
            };

            Guid publicId = await copyrightService.CreateAsync(userId, dto, cancellationToken);

            TempData["SuccessMessage"] = CopyrightAddedMessage;

            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(Details), new { id = publicId });
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet("Copyrights/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var dto = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);
            if (dto is null) return NotFound();

            var (workType, otherText) = TypeOfWorkMapper(dto.TypeOfWork);

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = dto.PublicId,
                RegistrationNumber = dto.RegistrationNumber,
                WorkType = workType,
                OtherWorkType = otherText,
                Title = dto.Title,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                Owner = dto.Owner,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };

            ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
            return View(viewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost("Copyrights/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CopyrightEditViewModel viewModel, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                ViewBag.YearOptions = YearOptionsProvider.BuildYearOptions();
                return Forbid();
            }

            // Conditional validation: OtherWorkType required when WorkType == Other
            if (viewModel.WorkType == CopyrightWorkType.Other && string.IsNullOrWhiteSpace(viewModel.OtherWorkType))
            {
                ModelState.AddModelError(nameof(CopyrightEditViewModel.OtherWorkType), "Please specify the work type.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var copyrightEditDTO = new CopyrightEditDTO
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                WorkType = viewModel.WorkType,
                OtherWorkType = viewModel.OtherWorkType,
                Title = viewModel.Title,
                YearOfCreation = viewModel.YearOfCreation,
                DateOfPublication = viewModel.DateOfPublication,
                Owner = viewModel.Owner,
                NationOfFirstPublication = viewModel.NationOfFirstPublication
            };

            var editedSuccessfully = await copyrightService.EditAsync(userId, id, copyrightEditDTO, cancellationToken);

            if (!editedSuccessfully)
            {
                ModelState.AddModelError(string.Empty, CopyrightUpdatesErrorMessage);
                return View(viewModel);
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

            var dto = await copyrightService.GetDetailsAsync(userId, id, cancellationToken);

            if (dto is null) return NotFound();

            var viewModel = new CopyrightDetailsViewModel
            {
                PublicId = dto.PublicId,
                RegistrationNumber = dto.RegistrationNumber,
                TypeOfWork = dto.TypeOfWork,
                Title = dto.Title,
                YearOfCreation = dto.YearOfCreation,
                DateOfPublication = dto.DateOfPublication,
                Owner = dto.Owner,
                NationOfFirstPublication = dto.NationOfFirstPublication
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await copyrightService.RemoveAsync(userId, id, cancellationToken);

            TempData["SuccessMessage"] = CopyrightRemovedMessage;
            return RedirectToLocal(returnUrl) ?? RedirectToAction(nameof(MyCollection));
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

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, publicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightDMCADetails(viewModel, copyrightDetailsDTO, MergeStrategy.OverwriteAll);

            var input = new DMCAInput(
                SenderName: viewModel.SenderName,
                SenderEmail: viewModel.SenderEmail,
                SenderAddress: viewModel.SenderAddress,
                RecipientName: viewModel.RecipientName,
                RecipientEmail: viewModel.RecipientEmail ?? string.Empty,
                RecipientAddress: viewModel.RecipientAddress ?? string.Empty,
                Date: DateTime.UtcNow,
                WorkTitle: viewModel.WorkTitle,
                RegistrationNumber: viewModel.RegistrationNumber ?? string.Empty,
                YearOfCreation: viewModel.YearOfCreation,
                DateOfPublication: viewModel.DateOfPublication,
                NationOfFirstPublication: viewModel.NationOfFirstPublication,
                InfringingUrl: viewModel.InfringingUrl,
                GoodFaithStatement: viewModel.GoodFaithStatement,
                BodyTemplate: viewModel.BodyTemplate
            );

            var pdf = await pdfService.GenerateCopyrightDMCAAsync(input, cancellationToken);

            return File(pdf, "application/pdf",
                $"DMCA-{copyrightDetailsDTO.Title}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(Guid publicId, CancellationToken cancellationToken = default)
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

            var viewModel = new DMCAViewModel { PublicId = publicId };

            ApplyCopyrightDMCADetails(viewModel, copyrightDetailsDTO, MergeStrategy.FillBlanks);

            var template = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ?? string.Empty;

            var placeholders = new Dictionary<string, string?>
            {
                ["Date"] = DateTime.UtcNow.ToString(DateTimeFormat),
                ["RecipientName"] = viewModel.RecipientName,
                ["RecipientAddress"] = viewModel.RecipientAddress,
                ["RecipientEmail"] = viewModel.RecipientEmail,
                ["SenderName"] = viewModel.SenderName,
                ["SenderAddress"] = viewModel.SenderAddress,
                ["SenderEmail"] = viewModel.SenderEmail,
                ["WorkTitle"] = viewModel.WorkTitle,
                ["RegistrationNumber"] = viewModel.RegistrationNumber,
                ["InfringingUrl"] = viewModel.InfringingUrl,
                ["YearOfCreation"] = viewModel.YearOfCreation?.ToString(),
                ["DateOfPublication"] = viewModel.DateOfPublication?.ToString(DateTimeFormat),
                ["NationOfFirstPublication"] = viewModel.NationOfFirstPublication,
                ["GoodFaithStatement"] = viewModel.GoodFaithStatement
            };

            viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
            return View("DMCAPreview", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> DmcaPreview(DMCAViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("DMCA", viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, viewModel.PublicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightDMCADetails(viewModel, copyrightDetailsDTO, MergeStrategy.FillBlanks);

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey("DMCA-General")?.BodyTemplate ?? viewModel.BodyTemplate;

                var placeholders = new Dictionary<string, string>
                {
                    ["Date"] = DateTime.UtcNow.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                    ["RecipientName"] = viewModel.RecipientName ?? "",
                    ["RecipientAddress"] = viewModel.RecipientAddress ?? "",
                    ["RecipientEmail"] = viewModel.RecipientEmail ?? "",
                    ["SenderName"] = viewModel.SenderName ?? "",
                    ["SenderAddress"] = viewModel.SenderAddress ?? "",
                    ["SenderEmail"] = viewModel.SenderEmail ?? "",
                    ["InfringingUrl"] = viewModel.InfringingUrl ?? "",
                    ["WorkTitle"] = viewModel.WorkTitle ?? "",
                    ["RegistrationNumber"] = viewModel.RegistrationNumber ?? "",                  
                    ["YearOfCreation"] = viewModel.YearOfCreation?.ToString() ?? "",
                    ["DateOfPublication"] = viewModel.DateOfPublication?.ToString(DateTimeFormat) ?? "",
                    ["NationOfFirstPublication"] = viewModel.NationOfFirstPublication ?? "",
                    ["GoodFaithStatement"] = viewModel.GoodFaithStatement ?? ""
                };

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return View("DMCAPreview", viewModel);
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult DmcaEdit(Guid publicId, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? 
                returnUrl : Url.Action(nameof(DmcaPreview), new { publicId })!;

            return View("DMCAEdit", new DMCAViewModel { PublicId = publicId });
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
            };

            viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey("CND-Copyright")!.BodyTemplate ?? string.Empty;
            ViewData["ShowAdditionalFacts"] = true;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(Guid publicId, CeaseDesistViewModel viewModel, CancellationToken cancellationToken = default)
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

            var pdf = await pdfService.GenerateCopyrightCeaseDesistAsync(input, cancellationToken);
            return File(pdf, "application/pdf", $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(Guid publicId, bool reset = false, CancellationToken cancellationToken = default)
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

            var viewModel = new CeaseDesistViewModel { PublicId = publicId };
            var template = letterTemplateProvider.GetTemplateByKey(key: "CND-Copyright")?.BodyTemplate ?? string.Empty;

            var placeholders = new Dictionary<string, string?>
            {
                ["Date"] = DateTime.UtcNow.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                ["RecipientName"] = viewModel.RecipientName ?? "",
                ["RecipientAddress"] = viewModel.RecipientAddress ?? "",
                ["RecipientEmail"] = viewModel.RecipientEmail ?? "",
                ["SenderName"] = viewModel.SenderName ?? "",
                ["SenderAddress"] = viewModel.SenderAddress ?? "",
                ["SenderEmail"] = viewModel.SenderEmail ?? "",
                ["InfringingUrl"] = viewModel.InfringingUrl ?? "",
                ["WorkTitle"] = viewModel.WorkTitle,
                ["RegistrationNumber"] = viewModel.RegistrationNumber,
                ["AdditionalFacts"] = viewModel.AdditionalFacts
            };

            viewModel.BodyTemplate = ReplaceTemplate(template, placeholders!);
            return View(viewName: "CeaseDesistPreview", viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesistPreview(CeaseDesistViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View("CeaseDesist", viewModel);
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var copyrightDetailsDTO = await copyrightService.GetDetailsAsync(userId, viewModel.PublicId, cancellationToken);

            if (copyrightDetailsDTO is null)
            {
                return NotFound();
            }

            ApplyCopyrightCeaseDesistDetails(viewModel, copyrightDetailsDTO, MergeStrategy.FillBlanks);

            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey("CND-Copyright")?.BodyTemplate ?? string.Empty;

                var placeholders = new Dictionary<string, string>
                {
                    ["Date"] = DateTime.UtcNow.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                    ["RecipientName"] = viewModel.RecipientName ?? "",
                    ["RecipientAddress"] = viewModel.RecipientAddress ?? "",
                    ["RecipientEmail"] = viewModel.RecipientEmail ?? "",
                    ["SenderName"] = viewModel.SenderName ?? "",
                    ["SenderAddress"] = viewModel.SenderAddress ?? "",
                    ["SenderEmail"] = viewModel.SenderEmail ?? "",
                    ["InfringingUrl"] = viewModel.InfringingUrl ?? "",
                    ["WorkTitle"] = viewModel.WorkTitle ?? "",
                    ["RegistrationNumber"] = viewModel.RegistrationNumber ?? "",
                    ["AdditionalFacts"] = viewModel.AdditionalFacts ?? ""
                };

                viewModel.BodyTemplate = ReplaceTemplate(template, placeholders);
            }

            return View("CeaseDesistPreview", viewModel);
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(Guid publicId, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? 
                returnUrl : Url.Action(nameof(CeaseDesistPreview), new { publicId })!;

            return View("CeaseDesistEdit", new CeaseDesistViewModel { PublicId = publicId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel model)
        {
            return View("CeaseDesistEdit", model);
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
    }
}
