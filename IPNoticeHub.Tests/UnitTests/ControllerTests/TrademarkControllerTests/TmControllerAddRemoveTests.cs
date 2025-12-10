using FluentAssertions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.StatusMessages.TrademarkStatusMessages;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    [TestFixture]
    public class TmControllerAddRemoveTests
    {
        [Test]
        public async Task Add_WithUserAndLocalReturnUrl_CallsService_SetsTempData_RedirectsToReturnUrl()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.AddAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                out var tempData, 
                userId: "u1");

            var addActionResult = await controller.Add(
                42, 
                "/local", 
                CancellationToken.None);

            tmCollectionService.Verify(s => s.AddAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            tempData["SuccessMessage"].Should().
                Be(TmAddedToCollectionMessage);

            var redirect = addActionResult.Should().
                BeOfType<LocalRedirectResult>().Subject;

            redirect.Url.Should().
                Be("/local");
        }

        [Test]
        public async Task Remove_WithUserAndNoReturnUrl_CallsService_SetsTempData_RedirectsToMyCollection()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.RemoveAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                out var tempData, 
                userId: "u1");

            var removeActionResult = await controller.Remove(
                42, 
                null, 
                CancellationToken.None);

            tmCollectionService.Verify(s => s.RemoveAsync(
                "u1", 
                42, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            tempData["SuccessMessage"].Should().
                Be(TmRemovedFromCollectionMessage);

            var redirect = removeActionResult as RedirectToActionResult;

            redirect.Should().
                NotBeNull();

            redirect!.ActionName.Should().
                Be(nameof(TrademarksController.MyCollection));
        }

        [Test]
        public async Task Add_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
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
        public async Task Remove_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                out var _, 
                userId: null);

            var removeActionResult = await controller.Remove(
                trademarkId: 42, 
                returnUrl: null, 
                CancellationToken.None);

            removeActionResult.Should().
                BeOfType<ForbidResult>();

            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WithNullReturnUrl_RedirectsToMyCollection()
        {
            const string userId = "u1";
            const int trademarkId = 42;

            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.AddAsync(
                userId, 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object,
                userId: userId);

            var addActionResult = await controller.Add(
                trademarkId, 
                null, 
                CancellationToken.None);

            var redirectResult = addActionResult.Should().
                BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().
                Be(nameof(TrademarksController.MyCollection));

            tmCollectionService.Verify(
                s => s.AddAsync(
                userId, 
                trademarkId, 
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
