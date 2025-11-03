using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.Implementations;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.Trademarks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    /// <summary>
    /// Section: TrademarksController – Index   
    ///  - Verifies that Index with valid search term calls ITrademarkSearchService.SearchAsync
    /// and returns populated view model with ViewBag.HasSearch = true.
    ///  - Verifies that Index with empty/whitespace search term returns view
    /// with empty page model and ViewBag.HasSearch = false.
    /// </summary>
    [TestFixture]
    public class TmControllerIndexTests
    {
        [Test]
        public async Task Index_WhenSearchTermIsEmpty_ReturnsEmptyViewModel_WithHasSearchFalse()
        {
            var tmSearchService = new Mock<ITrademarkSearchService>(MockBehavior.Strict);
            var tmCollectionService = new Mock<ITrademarkCollectionService>(MockBehavior.Strict);
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>(MockBehavior.Strict); // if your ctor needs it
            var pdfService = new Mock<IPdfService>();

            var controller = new TrademarksController
                (tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object, pdfService.Object);

            var filterViewModel = new TrademarkFilterViewModel
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "   ",
                ExactMatch = false,
                CurrentPage = 1,
                ResultsPerPage = 10
            };

            IActionResult indexResult = await controller.Index(filterViewModel, cancellationToken: default);

            var view = indexResult as ViewResult;
            view.Should().NotBeNull();

            view!.Model.Should().BeOfType<TrademarksIndexViewModel>();

            ((bool)controller.ViewBag.HasSearch).Should().BeFalse();

            tmSearchService.VerifyNoOtherCalls();
            tmCollectionService.VerifyNoOtherCalls();
            tmWatchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_WhenValidSearchTerm_CallsServiceAndReturnsPopulatedView_WithHasSearchTrue()
        {
            var pagedResult = new PagedResult<TrademarkSummaryDTO>
            {
                ResultsCount = 2,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new[]
               {
                    new TrademarkSummaryDTO { 
                        Id = 1, 
                        PublicId = Guid.NewGuid(),
                        Wordmark = "ALPHA",
                        Provider = DataProvider.USPTO,
                        Classes = new[] { 9 } },

                    new TrademarkSummaryDTO {
                        Id = 2, 
                        PublicId = Guid.NewGuid(),
                        Wordmark = "BETA",
                        Provider = DataProvider.EUIPO,
                        Classes = new[] { 25 } }
                }.ToList()
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();
            var pdfService = new Mock<IPdfService>();

            var filter = new TrademarkFilterDTO
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "alpha",
                ExactMatch = false
            };

            tmSearchService.Setup(service => service.SearchAsync(
                It.Is<TrademarkFilterDTO>(f =>
                    f.SearchBy == filter.SearchBy &&
                    f.SearchTerm == filter.SearchTerm &&
                    f.ExactMatch == filter.ExactMatch),
                1,
                10,
                It.IsAny<CancellationToken>())).ReturnsAsync(pagedResult);

            var controller = new TrademarksController
                (tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object, pdfService.Object);

            var filterViewModel = new TrademarkFilterViewModel
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = " alpha ",
                ExactMatch = false,
                CurrentPage = 1,
                ResultsPerPage = 10
            };

            var indexResult = await controller.Index(filterViewModel, default);

            var indexView = indexResult as ViewResult;

            indexView.Should().NotBeNull();
            ((bool)controller.ViewBag.HasSearch).Should().BeTrue();
            indexView.Model.Should().BeOfType<TrademarksIndexViewModel>();

            tmSearchService.Verify(s => s.SearchAsync(
                It.IsAny<TrademarkFilterDTO>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);

            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_WhenSearchTermIsNull_ReturnsEmptyViewModel_WithHasSearchFalse()
        {
            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();
            var pdfService = new Mock<IPdfService>();

            var controller = new TrademarksController
                (tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object, pdfService.Object);

            var filterViewModel = new TrademarkFilterViewModel
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = null,
                ExactMatch = false,
                CurrentPage = 1,
                ResultsPerPage = 10
            };

            var indexActionResult = await controller.Index(filterViewModel, default);

            var indexView = indexActionResult as ViewResult;

            indexView.Should().NotBeNull();
            ((bool)controller.ViewBag.HasSearch).Should().BeFalse();
            indexView!.Model.Should().BeOfType<TrademarksIndexViewModel>();

            tmSearchService.VerifyNoOtherCalls();
            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_WhenClassNumbersNull_NormalizesToEmptyArray()
        {
            TrademarkFilterDTO? filterDTO = null;

            var pagedResult = new PagedResult<TrademarkSummaryDTO>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<TrademarkSummaryDTO>()
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();
            var pdfService = new Mock<IPdfService>();

            tmSearchService.Setup(s => s.SearchAsync(
                It.IsAny<TrademarkFilterDTO>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>())).
            Callback((TrademarkFilterDTO filter, int _, int _, CancellationToken __) =>
            {
                filterDTO = filter;
            }).
            ReturnsAsync(pagedResult);

            var controller = new TrademarksController
                (tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object, pdfService.Object);

            var filterViewModel = new TrademarkFilterViewModel
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "alpha",
                ExactMatch = true,
                CurrentPage = 1,
                ResultsPerPage = 10,
                ClassNumbers = Array.Empty<int>()
            };

            await controller.Index(filterViewModel, default);

            filterDTO.Should().NotBeNull();
            filterDTO!.ClassNumbers.Should().NotBeNull();
            filterDTO.ClassNumbers.Should().BeEmpty();
            filterDTO.ExactMatch.Should().BeTrue();
        }

        [Test]
        public async Task Index_WhenAllClassNumbersInvalid_NormalizesToEmptyArray()
        {
            TrademarkFilterDTO? filterDTO = null;

            var pagedResult = new PagedResult<TrademarkSummaryDTO>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<TrademarkSummaryDTO>()
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();
            var pdfService = new Mock<IPdfService>();

            tmSearchService.Setup(s => s.SearchAsync(
                It.IsAny<TrademarkFilterDTO>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>())).
            Callback((TrademarkFilterDTO filter, int _, int _, CancellationToken __) =>
            {
                filterDTO = filter;
            }).
            ReturnsAsync(pagedResult);

            var controller = new TrademarksController
                (tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object, pdfService.Object);

            var filterViewModel = new TrademarkFilterViewModel
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "alpha",
                ExactMatch = false,
                CurrentPage = 1,
                ResultsPerPage = 10,
                ClassNumbers = new[] { -1, 0, 999 }
            };

            await controller.Index(filterViewModel, default);

            filterDTO.Should().NotBeNull();
            filterDTO!.ClassNumbers.Should().NotBeNull();
            filterDTO.ClassNumbers.Should().BeEmpty();
            filterDTO.ExactMatch.Should().BeFalse();
        }
    }
}
