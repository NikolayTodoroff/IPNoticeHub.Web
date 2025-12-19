using FluentAssertions;
using IPNoticeHub.Application.Services.TrademarkService.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.StatusMessages.TrademarkStatusMessages;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmControllerTests
{
    public class RemoveTmControllerTests
    {
        [Test]
        public async Task Remove_CallsService_SetsTempData_RedirectsToMyCollection()
        {
            var tmCollectionService =
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(
                s => s.RemoveAsync(
                "u1",
                42,
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var controller =
                TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object,
                out var tempData,
                userId: "u1");

            var removeActionResult =
                await controller.Remove(
                42,
                null,
                CancellationToken.None);

            tmCollectionService.Verify(
                s => s.RemoveAsync(
                "u1",
                42,
                It.IsAny<CancellationToken>()),
                Times.Once);

            tempData["SuccessMessage"].Should().Be(TmRemovedFromCollectionMessage);

            var redirect = removeActionResult as RedirectToActionResult;

            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be(nameof(TrademarksController.MyCollection));
        }

        [Test]
        public async Task Remove_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService =
                new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller =
                TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object,
                out var _,
                userId: null);

            var removeActionResult = await controller.Remove(
                trademarkId: 42,
                returnUrl: null,
                CancellationToken.None);

            removeActionResult.Should().BeOfType<ForbidResult>();
            tmCollectionService.VerifyNoOtherCalls();
        }
    }
}
