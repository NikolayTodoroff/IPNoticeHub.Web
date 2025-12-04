using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Infrastructure;
using IPNoticeHub.Web.Models.PdfGeneration;
using IPNoticeHub.Web.Models.Trademarks;
using IPNoticeHub.Web.ViewModels.Trademarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using static IPNoticeHub.Web.Infrastructure.TemplateReplacer;
using static IPNoticeHub.Web.Infrastructure.ApplyEntityDetails;
using IPNoticeHub.Web.Models.TrademarkCollection;

namespace IPNoticeHub.Web.Controllers
{
    public sealed class TrademarksController : Controller
    {
        private readonly ITrademarkCollectionService tmCollectionService;
        private readonly ITrademarkSearchService tmSearchService;
        private readonly ITrademarkWatchlistService tmWatchlistService;
        private readonly IPdfService pdfService;
        private readonly ILetterTemplateProvider letterTemplateProvider;

        public TrademarksController(ITrademarkSearchService searchService, ITrademarkCollectionService collectionService, 
            ITrademarkWatchlistService tmWatchlistService, IPdfService pdfService, ILetterTemplateProvider letterTemplateProvider)
        {
            this.tmSearchService = searchService;
            this.tmCollectionService = collectionService;
            this.tmWatchlistService = tmWatchlistService;
            this.pdfService = pdfService;
            this.letterTemplateProvider = letterTemplateProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TrademarkFilterViewModel filter, CancellationToken cancellationToken)
        {
            string searchTerm = (filter.SearchTerm ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(searchTerm) || !ModelState.IsValid)
            {
                return CreateEmptyViewModel(filter);
            }

            var filterDTO = CreateNormalizedFilterDTO(filter, searchTerm);

            var pagedResult = await tmSearchService.SearchAsync(filterDTO, filter.CurrentPage, filter.ResultsPerPage, cancellationToken);

            ViewBag.HasSearch = true;

            var viewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, pagedResult);
            return View(viewModel);
        }

        [HttpGet("Trademarks/Details/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            TrademarkDetailsDTO? detailsDTO = await tmSearchService.GetDetailsAsync(id, cancellationToken);

            if (detailsDTO is null) return NotFound();

            bool isAuthenticated = User.Identity?.IsAuthenticated == true;
            bool isInCollection = false;
            bool isInWatchlist = false;

            if (isAuthenticated && User.TryGetUserId(out var userId))
            {
                isInCollection = await tmCollectionService.IsInCollectionAsync(userId, detailsDTO.Id,false,cancellationToken);
                isInWatchlist = await tmWatchlistService.ExistsAsync(userId, detailsDTO.Id, cancellationToken);
            }

            var detailsViewModel = new TrademarkDetailsViewModel
            {
                Id = detailsDTO.Id,
                PublicId = detailsDTO.PublicId,
                Wordmark = detailsDTO.Wordmark,
                Owner = detailsDTO.Owner,
                SourceId = detailsDTO.SourceId,
                RegistrationNumber = detailsDTO.RegistrationNumber,
                Status = detailsDTO.Status,
                GoodsAndServices = detailsDTO.GoodsAndServices,
                FilingDate = detailsDTO.FilingDate,
                RegistrationDate = detailsDTO.RegistrationDate,
                MarkImageUrl = detailsDTO.MarkImageUrl,
                Provider = detailsDTO.Provider,
                Classes = detailsDTO.Classes,
                IsInCollection = isInCollection,
                IsInWatchlist = isInWatchlist,
                IsAuthenticated = isAuthenticated
            };

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }

            return View(detailsViewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpGet]
        public async Task<IActionResult> MyCollection(CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc,
        int currentPage = DefaultPage,int resultsPerPage = DefaultPageSize,CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            var pagedResult = await tmCollectionService.
                GetUserCollectionAsync(userId, sortBy, currentPage, resultsPerPage, cancellationToken);

            var viewModel = TrademarkCollectionDtoToVmMapper.Map(pagedResult);

            ViewBag.SortBy = sortBy;

            return View(viewModel);
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Add(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            try
            {
                if (await tmCollectionService.IsInCollectionAsync(userId, trademarkId, false, cancellationToken))
                {
                    TempData["InfoMessage"] = "Trademark is already in your collection.";
                    return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
                }

                await tmCollectionService.AddAsync(userId, trademarkId, cancellationToken);
                TempData["SuccessMessage"] = TmAddedToCollectionMessage;
                return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
            }
            catch
            {
                TempData["ErrorMessage"] = TmAddToCollectionErrorMessage;
                return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
            }
        }

        [Authorize(Policy = "HasUserId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int trademarkId, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId)) return Forbid();

            await tmCollectionService.RemoveAsync(userId, trademarkId, cancellationToken);

            TempData["SuccessMessage"] = TmRemovedFromCollectionMessage;
            return this.RedirectToLocalOrAction(returnUrl, nameof(MyCollection));
        }

        [HttpGet,Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(Guid publicId, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var trademarkDetailsDTO = await tmSearchService.GetDetailsAsync(publicId, cancellationToken);

            if (trademarkDetailsDTO == null)
            {
                return NotFound();
            }

            bool isInCollection = await tmCollectionService.IsInCollectionAsync(userId, trademarkDetailsDTO.Id, false, cancellationToken);

            if (!isInCollection)
            {
                return NotFound();
            }

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = trademarkDetailsDTO.Wordmark ?? trademarkDetailsDTO.RegistrationNumber ?? "Trademark",
                RegistrationNumber = trademarkDetailsDTO.RegistrationNumber,
            };

            viewModel.BodyTemplate = letterTemplateProvider.GetTemplateByKey("CND-Trademark")?.BodyTemplate ?? string.Empty;
            ViewData["ShowAdditionalFacts"] = true;

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public async Task<IActionResult> CeaseDesist(Guid publicId,CeaseDesistViewModel viewModel, CancellationToken cancellationToken = default)
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

            var pdf = await pdfService.GenerateTrademarkCeaseDesistAsync(input, cancellationToken);
            return File(pdf, "application/pdf", $"CeaseDesist-{viewModel.WorkTitle}-{DateTime.UtcNow:DateTimeFormat}.pdf");
        }

        [HttpGet, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistPreview(CeaseDesistViewModel viewModel)
        {
            if (!User.TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var template = letterTemplateProvider.GetTemplateByKey(key: "CND-Trademark")?.BodyTemplate ?? string.Empty;

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

            var trademarkDetailsDTO = await tmSearchService.GetDetailsAsync(viewModel.PublicId, cancellationToken);

            if (trademarkDetailsDTO != null)
            {
                ApplyTrademarkCeaseDesistDetails(viewModel, trademarkDetailsDTO, MergeStrategy.FillBlanks);
            }
                
            if (string.IsNullOrWhiteSpace(viewModel.BodyTemplate) || viewModel.BodyTemplate.Contains("{{"))
            {
                var template = letterTemplateProvider.GetTemplateByKey("CND-Trademark")?.BodyTemplate ?? string.Empty;

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
        public IActionResult CeaseDesistEdit(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl : Url.Action(nameof(CeaseDesistPreview))!;

            return View(viewName: "CeaseDesistEdit", model: new CeaseDesistViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "HasUserId")]
        public IActionResult CeaseDesistEdit(CeaseDesistViewModel model)
        {
            return View("CeaseDesistEdit", model);
        }

        // Helper methods for internal use within the controller
        private IActionResult CreateEmptyViewModel(TrademarkFilterViewModel filter)
        {
            ViewBag.HasSearch = false;
            PagedResult<TrademarkSummaryDTO>? emptyDTOPagedResult = new PagedResult<TrademarkSummaryDTO>
            {
                Results = Array.Empty<TrademarkSummaryDTO>(),
                ResultsCount = 0,
                CurrentPage = filter.CurrentPage,
                ResultsCountPerPage = filter.ResultsPerPage
            };

            TrademarksIndexViewModel? viewModel = TrademarksIndexDtoToVmMapper.MapToIndexViewModel(filter, emptyDTOPagedResult);
            return View(viewModel);
        }
        private static TrademarkFilterDTO CreateNormalizedFilterDTO(TrademarkFilterViewModel filter, string searchTerm)
        {
            return new TrademarkFilterDTO
            {
                SearchTerm = searchTerm,
                SearchBy = filter.SearchBy,
                Provider = filter.Provider,
                Status = filter.Status,
                ClassNumbers = (filter.ClassNumbers ?? Array.Empty<int>())
                        .Where(cn => Enum.IsDefined(typeof(TrademarkClass), cn))
                        .Distinct()
                        .ToArray(),
                ExactMatch = filter.ExactMatch
            };
        }
    }
}
