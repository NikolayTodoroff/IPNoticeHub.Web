using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Tests.UnitTests.UnitTestUtilities;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.ViewModels.Trademarks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    /// <summary>
    /// Section: TrademarksController – Details
    ///  - Verifies that Details(publicId) returns ViewResult with TrademarkDetailsDTO model when found.
    ///  - Verifies that Details(publicId) returns NotFoundResult when not found.
    ///  - Verifies that Details sets the ReturnUrl in ViewData when the provided URL is local.
    ///  - Verifies that Details does not set the ReturnUrl in ViewData when the provided URL is external.
    /// </summary>
    [TestFixture]
    public class TmControllerDetailsTests
    {
        [Test]
        public async Task Details_WhenPublicIdExists_ReturnsViewWithViewModel()
        {
            var entityId = Guid.NewGuid();

            var detailsDTO = new TrademarkDetailsDTO
            {
                Id = 42,
                PublicId = entityId,
                Wordmark = "Target",
                Owner = "Owner A",
                Provider = DataProvider.USPTO,
                Classes = new[] { 9, 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();

            tmSearchService
                .Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(detailsDTO);

            tmWatchlistService
                .Setup(s => s.ExistsAsync("user-123", 42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new TrademarksController(
                tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await controller.Details(entityId, null, CancellationToken.None);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            viewResult!.Model.Should().BeOfType<TrademarkDetailsViewModel>();
            var vm = (TrademarkDetailsViewModel)viewResult.Model!;

            vm.PublicId.Should().Be(entityId);
            vm.Wordmark.Should().Be("Target");
            vm.Id.Should().Be(42);
            vm.Owner.Should().Be("Owner A");
            vm.Classes.Should().BeEquivalentTo(new[] { 9, 25 });
            vm.IsInWatchlist.Should().BeFalse();

            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
            tmWatchlistService.Verify(s => s.ExistsAsync("user-123", 42, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Details_WhenPublicIdDoesNotExist_ReturnsNotFound()
        {
            var entityId = Guid.NewGuid();

            var tmSearchService = new Mock<ITrademarkSearchService>();
            var tmCollectionService = new Mock<ITrademarkCollectionService>();
            var tmWatchlistService = new Mock<ITrademarkWatchlistService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>())).
                ReturnsAsync((TrademarkDetailsDTO?)null);


            var controller = new TrademarksController(tmSearchService.Object, tmCollectionService.Object, tmWatchlistService.Object);

            var detailsResult = await controller.Details(entityId, default);

            detailsResult.Should().BeOfType<NotFoundResult>();
            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Details_WhenReturnUrlIsLocal_SetsReturnUrlInViewData()
        {
            var entityId = Guid.NewGuid();
            var safeReturnUrl = "/Home/Results?trademark=chimaira";

            var detailsDTO = new TrademarkDetailsDTO
            {
                PublicId = entityId,
                Wordmark = "Chimaira",
                Owner = "Armstrong LLC",
                Provider = DataProvider.USPTO,
                Classes = new[] { 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            tmSearchService
                .Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(detailsDTO);

            var collectionService = new Mock<ITrademarkCollectionService>();
            
            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                collectionService: collectionService.Object,
                out _,
                searchService: tmSearchService.Object,
                watchlistService: null,
                userId: "user-1"
            );

            controller.ConfigureUrlHelper(returnUrl: safeReturnUrl, isLocal: true);

            var actionResult = await controller.Details(entityId, safeReturnUrl, CancellationToken.None);

            var view = actionResult as ViewResult;
            view.Should().NotBeNull();
            view!.Model.Should().BeOfType<TrademarkDetailsViewModel>();

            view.ViewData["ReturnUrl"].Should().NotBeNull();
            view.ViewData["ReturnUrl"]!.ToString().Should().Be(safeReturnUrl);

            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Details_WhenReturnUrlIsExternal_DoesNotSetReturnUrl()
        {
            var entityId = Guid.NewGuid();
            var externalUrl = "https://custom.example/demo";

            var detailsDTO = new TrademarkDetailsDTO
            {
                PublicId = entityId,
                Wordmark = "Chimaira",
                Owner = "Armstrong LLC",
                Provider = DataProvider.USPTO,
                Classes = new[] { 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();
            tmSearchService.Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(detailsDTO);

            var collectionService = new Mock<ITrademarkCollectionService>();

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                collectionService: collectionService.Object,
                out _,
                searchService: tmSearchService.Object,
                watchlistService: null,
                userId: "user-1"
            );

            controller.ConfigureUrlHelper(returnUrl: externalUrl, isLocal: false);

            var actionResult = await controller.Details(entityId, externalUrl, default);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();
            resultView!.Model.Should().BeOfType<TrademarkDetailsViewModel>();

            // Unsafe URL should be ignored (no ReturnUrl provided to the View)
            resultView.ViewData.ContainsKey("ReturnUrl").Should().BeFalse();

            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

