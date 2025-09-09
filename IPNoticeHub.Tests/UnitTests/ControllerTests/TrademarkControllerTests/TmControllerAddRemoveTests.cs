using FluentAssertions;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    /// <summary>
    /// Section: TrademarksController – Add/Remove
    /// - Verifies that Add and Remove with provided valid user id call service,
    /// sets TempData status, redirects to local returnUrl (if provided) or MyCollection.
    /// - Verifies that Add and Remove without provided valid user id return Forbid() .
    /// </summary>
    [TestFixture]
    public class TmControllerAddRemoveTests
    {
        [Test]
        public async Task Add_WithUserAndLocalReturnUrl_CallsService_SetsTempData_RedirectsToReturnUrl()
        {
            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.AddAsync(
                "u1", 42, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var controller = TestControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, out var tempData, userId: "u1");

            var addActionResult = await controller.Add(42, "/local", CancellationToken.None);

            tmCollectionService.Verify(s => s.AddAsync("u1", 42, It.IsAny<CancellationToken>()), Times.Once);

            tempData["StatusMessage"].Should().Be(TrademarkAddedMessage);

            var redirectResult = addActionResult as RedirectResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.Url.Should().Be("/local");
        }

        [Test]
        public async Task Remove_WithUserAndNoReturnUrl_CallsService_SetsTempData_RedirectsToMyCollection()
        {
            var tmCollectionService = new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.RemoveAsync(
                "u1", 42, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var controller = TestControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, out var tempData, userId: "u1");

            var removeActionResult = await controller.Remove(
                42, null, CancellationToken.None);

            tmCollectionService.Verify(s => s.RemoveAsync("u1", 42, It.IsAny<CancellationToken>()), Times.Once);

            tempData["StatusMessage"].Should().Be(TrademarkRemovedMessage);

            var redirect = removeActionResult as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be(nameof(IPNoticeHub.Web.Controllers.TrademarksController.MyCollection));
        }

        [Test]
        public async Task Add_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService = new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = TestControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, out var _, userId: null);

            var addActionResult = await controller.Add(trademarkId: 42, returnUrl: null, CancellationToken.None);

            addActionResult.Should().BeOfType<ForbidResult>();
            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Remove_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService = new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = TestControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, out var _, userId: null);

            var removeActionResult = await controller.Remove(trademarkId: 42, returnUrl: null, CancellationToken.None);

            removeActionResult.Should().BeOfType<ForbidResult>();
            tmCollectionService.VerifyNoOtherCalls();
        }
    }
}
