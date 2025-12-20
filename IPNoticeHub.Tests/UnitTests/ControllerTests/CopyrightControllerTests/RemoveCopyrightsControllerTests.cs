using FluentAssertions;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.StatusMessages.CopyrightStatusMessages;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class RemoveCopyrightsControllerTests
    {
        [Test]
        public async Task Remove_WithNoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: null);

            var actionResult = 
                await controller.Remove(Guid.NewGuid());

            actionResult.Should().
                BeOfType<ForbidResult>();

            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Remove_LocalReturnUrl_RedirectsThere_AndSetsTempData()
        {
            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = 
                await controller.Remove(
                Guid.NewGuid(), 
                returnUrl: "/back");

            var redirect = 
                actionResult.Should().BeOfType<RedirectResult>().Subject;

            redirect.Url.Should().Be("/back");
            controller.TempData["SuccessMessage"].Should().Be(CopyrightRemovedMessage);

            service.Verify(s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Remove_NoReturnUrl_RedirectsToMyCollection_AndSetsTempData()
        {
            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = 
                await controller.Remove(
                Guid.NewGuid(), 
                returnUrl: null);

            var redirect = 
                actionResult.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().
                Be(nameof(CopyrightsController.MyCollection));

           controller.TempData["SuccessMessage"].Should().Be(CopyrightRemovedMessage);

            service.Verify(
                s => s.RemoveAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
