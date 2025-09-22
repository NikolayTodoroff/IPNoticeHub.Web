using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

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
        public async Task Details_WhenPublicIdExists_ReturnsViewWithDetailsModel()
        {
            var entityId = Guid.NewGuid();

            var detailsDTO = new TrademarkDetailsDTO
            {
                PublicId = entityId,
                Wordmark = "Target",
                Owner = "Owner A",
                Provider = DataProvider.USPTO,
                Classes = new[] { 9, 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>())).
                ReturnsAsync(detailsDTO);

            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            var controller = new TrademarksController(tmSearchService.Object, tmCollectionService.Object);

            var detailsResult = await controller.Details(entityId, default);

            var viewResult = detailsResult as ViewResult;

            viewResult.Should().NotBeNull();

            viewResult!.Model.Should().BeOfType<TrademarkDetailsDTO>();

            var viewModel = viewResult.Model as TrademarkDetailsDTO;

            viewModel!.PublicId.Should().Be(entityId);
            viewModel.Wordmark.Should().Be("Target");

            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Details_WhenPublicIdDoesNotExist_ReturnsNotFound()
        {
            var entityId = Guid.NewGuid();

            var tmSearchService = new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>())).
                ReturnsAsync((TrademarkDetailsDTO?)null);

            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            var controller = new TrademarksController(tmSearchService.Object, tmCollectionService.Object);

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
            tmSearchService.Setup(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(detailsDTO);

            var collectionService = new Mock<ITrademarkCollectionService>();

            // Including UrlHelper via the factory overload with TempData/Url enabled
            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                collectionService.Object,
                out _,
                tmSearchService.Object,
                userId: null
            );

            var actionResult = await controller.Details(entityId, safeReturnUrl, default);

            var view = actionResult as ViewResult;
            view.Should().NotBeNull();
            view!.Model.Should().BeOfType<TrademarkDetailsDTO>();


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
                collectionService.Object,
                out _,
                tmSearchService.Object,
                userId: null
            );

            var actionResult = await controller.Details(entityId, externalUrl, default);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();
            resultView!.Model.Should().BeOfType<TrademarkDetailsDTO>();

            // Unsafe URL should be ignored (no ReturnUrl provided to the View)
            resultView.ViewData.ContainsKey("ReturnUrl").Should().BeFalse();

            tmSearchService.Verify(s => s.GetDetailsAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

