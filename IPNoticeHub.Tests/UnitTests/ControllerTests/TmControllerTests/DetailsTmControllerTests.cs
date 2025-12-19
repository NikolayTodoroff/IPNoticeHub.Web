using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.ViewModels.Trademarks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmControllerTests
{
    public class DetailsTmControllerTests
    {
        [Test]
        public async Task Details_WhenPublicIdExists_ReturnsViewWithViewModel()
        {
            var entityId = Guid.NewGuid();

            var dto = new TrademarkDetailsDto
            {
                Id = 42,
                PublicId = entityId,
                Wordmark = "Target",
                Owner = "Owner A",
                Provider = DataProvider.USPTO,
                Classes = new[] { 9, 25 }
            };

            var tmSearchService = 
                new Mock<ITrademarkSearchService>();

            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            var tmWatchlistService = 
                new Mock<IWatchlistService>();

            var pdfService = 
                new Mock<IPdfLetterService>();

            var letterTemplate = 
                new Mock<ILetterTemplateProvider>();

            var documentLibraryService = 
                new Mock<IDocumentLibraryService>();

            tmSearchService.Setup(
                s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(dto);

            tmWatchlistService.Setup(
                s => s.ExistsAsync(
                "user-123", 
                42, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            var controller = new TrademarksController(
                tmSearchService.Object, 
                tmCollectionService.Object, 
                tmWatchlistService.Object, 
                pdfService.Object, 
                letterTemplate.Object, 
                documentLibraryService.Object);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] {
                        new Claim(ClaimTypes.NameIdentifier,"user-123") }
                    ,"TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await controller.Details(
                entityId, 
                null, 
                CancellationToken.None);

            var viewResult = result as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<TrademarkDetailsViewModel>();

            var viewModel = 
                (TrademarkDetailsViewModel)viewResult.Model!;

            viewModel.PublicId.Should().Be(entityId);
            viewModel.Wordmark.Should().Be("Target");
            viewModel.Id.Should().Be(42);
            viewModel.Owner.Should().Be("Owner A");
            viewModel.Classes.Should().BeEquivalentTo(new[] { 9, 25 });
            viewModel.IsInWatchlist.Should().BeFalse();

            tmSearchService.Verify(
                s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            tmWatchlistService.Verify(
                s => s.ExistsAsync(
                "user-123", 
                42, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Details_WhenPublicIdDoesNotExist_ReturnsNotFound()
        {
            var entityId = Guid.NewGuid();

            var tmSearchService = 
                new Mock<ITrademarkSearchService>();

            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            var tmWatchlistService = 
                new Mock<IWatchlistService>();

            var pdfService = 
                new Mock<IPdfLetterService>();

            var letterTemplate = 
                new Mock<ILetterTemplateProvider>();

            var documentLibraryService = 
                new Mock<IDocumentLibraryService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((TrademarkDetailsDto?)null);

            var controller = new TrademarksController
                (tmSearchService.Object, 
                tmCollectionService.Object, 
                tmWatchlistService.Object, 
                pdfService.Object, 
                letterTemplate.Object, 
                documentLibraryService.Object);

            var detailsResult = await controller.Details(
                entityId, 
                default);

            detailsResult.Should().BeOfType<NotFoundResult>();

            tmSearchService.Verify(s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Details_WhenReturnUrlIsLocal_SetsReturnUrlInViewData()
        {
            var entityId = Guid.NewGuid();
            var safeReturnUrl = "/Home/Results?trademark=chimaira";

            var dto = new TrademarkDetailsDto
            {
                PublicId = entityId,
                Wordmark = "Chimaira",
                Owner = "Armstrong LLC",
                Provider = DataProvider.USPTO,
                Classes = new[] { 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(
                s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(dto);

            var collectionService = 
                new Mock<ITrademarkCollectionService>();

            var documentLibraryService = 
                new Mock<IDocumentLibraryService>();

            var controller = 
                TestTrademarkControllerFactory.CreateTrademarksController(
                collectionService: collectionService.Object,
                out _,
                searchService: tmSearchService.Object,
                watchlistService: null,
                userId: "user-1",
                documentLibraryService: documentLibraryService.Object
            );

            controller.ConfigureUrlHelper(
                returnUrl: safeReturnUrl, 
                isLocal: true);

            var actionResult = await controller.Details(
                entityId, 
                safeReturnUrl, 
                CancellationToken.None);

            var view = actionResult as ViewResult;

            view.Should().NotBeNull();
            view!.Model.Should().BeOfType<TrademarkDetailsViewModel>();
            view.ViewData["ReturnUrl"].Should().NotBeNull();
            view.ViewData["ReturnUrl"]!.ToString().Should().Be(safeReturnUrl);

            tmSearchService.Verify(s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Details_WhenReturnUrlIsExternal_DoesNotSetReturnUrl()
        {
            var entityId = Guid.NewGuid();
            var externalUrl = "https://custom.example/demo";

            var detailsDTO = new TrademarkDetailsDto
            {
                PublicId = entityId,
                Wordmark = "Chimaira",
                Owner = "Armstrong LLC",
                Provider = DataProvider.USPTO,
                Classes = new[] { 25 }
            };

            var tmSearchService = 
                new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(detailsDTO);

            var collectionService = 
                new Mock<ITrademarkCollectionService>();

            var documentLibraryService = 
                new Mock<IDocumentLibraryService>();

            var controller = 
                TestTrademarkControllerFactory.CreateTrademarksController(
                collectionService: collectionService.Object,
                out _,
                searchService: tmSearchService.Object,
                watchlistService: null,
                userId: "user-1",
                documentLibraryService: documentLibraryService.Object
            );

            controller.ConfigureUrlHelper(
                returnUrl: externalUrl, 
                isLocal: false);

            var actionResult = await controller.Details(
                entityId, 
                externalUrl, 
                default);

            var resultView = actionResult as ViewResult;

            resultView.Should().NotBeNull();
            resultView!.Model.Should().BeOfType<TrademarkDetailsViewModel>();
            resultView.ViewData.ContainsKey("ReturnUrl").Should().BeFalse();

            tmSearchService.Verify(s => s.GetDetailsAsync(
                entityId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}

