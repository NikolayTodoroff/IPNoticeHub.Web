using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class CopyrightsControllerRemoveTests
    {
        [Test]
        public async Task Remove_WithNoUser_ReturnsForbid()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: null);

            var removeActionResult = await controller.Remove(Guid.NewGuid());

            removeActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Remove_LocalReturnUrl_RedirectsThere_AndSetsTempData()
        {
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var removeActionResult = await controller.Remove(
                Guid.NewGuid(), 
                returnUrl: "/back");

            var redirectResult = removeActionResult.Should().
                BeOfType<RedirectResult>().Subject;

            redirectResult.Url.Should().
                Be("/back");

            controller.TempData["SuccessMessage"].Should().
                Be(CopyrightRemovedMessage);

            copyrightService.Verify(s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Remove_NoReturnUrl_RedirectsToMyCollection_AndSetsTempData()
        {
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var removeActionResult = await controller.Remove(
                Guid.NewGuid(), 
                returnUrl: null);

            var redirectToActionResult = removeActionResult.Should().
                BeOfType<RedirectToActionResult>().Subject;

            redirectToActionResult.ActionName.Should().
                Be(nameof(CopyrightsController.MyCollection));

           controller.TempData["SuccessMessage"].Should().
                Be(CopyrightRemovedMessage);

            copyrightService.Verify(s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
