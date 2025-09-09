using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    /// <summary>
    /// Section: TrademarksController – Details
    /// - Verifies that Details(publicId) returns ViewResult with TrademarkDetailsDTO model when found.
    /// - Verifies that Details(publicId) returns NotFoundResult when not found.
    /// </summary>
    [TestFixture]
    public class TmControllerDetailsTests
    {
        [Test]
        public async Task Details_WhenPublicIdExists_ReturnsViewWithDetailsModel()
        {
            var id = Guid.NewGuid();

            var detailsDTO = new TrademarkDetailsDTO
            {
                PublicId = id,
                Wordmark = "Target",
                Owner = "Owner A",
                Provider = DataProvider.USPTO,
                Classes = new[] { 9, 25 }
            };

            var tmSearchService = new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(id, It.IsAny<CancellationToken>())).
                ReturnsAsync(detailsDTO);

            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            var controller = new TrademarksController(tmSearchService.Object, tmCollectionService.Object);

            var detailsResult = await controller.Details(id, default);

            var viewResult = detailsResult as ViewResult;

            viewResult.Should().NotBeNull();

            viewResult!.Model.Should().BeOfType<TrademarkDetailsDTO>();

            var viewModel = viewResult.Model as TrademarkDetailsDTO;

            viewModel!.PublicId.Should().Be(id);
            viewModel.Wordmark.Should().Be("Target");

            tmSearchService.Verify(s => s.GetDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Details_WhenPublicIdDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();

            var tmSearchService = new Mock<ITrademarkSearchService>();

            tmSearchService.Setup(s => s.GetDetailsAsync(id, It.IsAny<CancellationToken>())).
                ReturnsAsync((TrademarkDetailsDTO?)null);

            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            var controller = new TrademarksController(tmSearchService.Object, tmCollectionService.Object);

            var detailsResult = await controller.Details(id, default);

            detailsResult.Should().BeOfType<NotFoundResult>();
            tmSearchService.Verify(s => s.GetDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
