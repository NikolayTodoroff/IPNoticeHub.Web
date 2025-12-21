using FluentAssertions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.StatusMessages.TrademarkStatusMessages;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmControllerTests
{
    public class AddTmControllerTests
    {
        [Test]
        public async Task Add_CallsService_SetsTempData_RedirectsToReturnUrl()
        {
            var service = 
                new Mock<ITrademarkCollectionService>();

            service.Setup(
                s => s.AddAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller = 
                TestTrademarkControllerFactory.CreateTrademarksController(
                service.Object, 
                out var tempData, 
                userId: "u1");

            var result = await controller.Add(
                42, 
                "/local", 
                CancellationToken.None);

            service.Verify(
                s => s.AddAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            tempData["SuccessMessage"].Should().Be(TmAddedToCollectionMessage);

            var redirect = 
                result.Should().BeOfType<LocalRedirectResult>().Subject;

            redirect.Url.Should().Be("/local");
        }
        
        [Test]
        public async Task Add_WithMissingUser_ReturnsForbid()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = 
                TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                out var _, 
                userId: null);

            var addActionResult = await controller.Add(
                trademarkId: 42, 
                returnUrl: null, 
                CancellationToken.None);

            addActionResult.Should().
                BeOfType<ForbidResult>();

            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WithNullReturnUrl_RedirectsToMyCollection()
        {
            const string userId = "u1";
            const int trademarkId = 42;

            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(
                s => s.AddAsync(
                userId, 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller = 
                TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object,
                userId: userId);

            var addActionResult = await controller.Add(
                trademarkId, 
                null, 
                CancellationToken.None);

            var redirectResult = 
                addActionResult.Should().BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().Be(nameof(TrademarksController.MyCollection));

            tmCollectionService.Verify(
                s => s.AddAsync(
                userId, 
                trademarkId, 
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Add_WhenServiceThrows_SetsErrorMessage_AndRedirectsToMyCollection()
        {
            var collectionServiceMock =
                new Mock<ITrademarkCollectionService>();

            const int trademarkId = 42;
            const string userId = "user-123";

            collectionServiceMock.Setup(
                s => s.IsInCollectionAsync(
                    userId,
                    trademarkId,
                    false,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(false);

            collectionServiceMock.Setup(
                s => s.AddAsync(
                    userId,
                    trademarkId,
                    It.IsAny<CancellationToken>())).
                    ThrowsAsync(new Exception("Database error"));

            var controller =
                TestTrademarkControllerFactory.CreateTrademarksController(
                    collectionServiceMock.Object,
                    out var tempData,
                    userId: userId);

            var result = await controller.Add(trademarkId);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(TrademarksController.MyCollection));
            tempData.ContainsKey("ErrorMessage").Should().BeTrue();
        }
    }
}
