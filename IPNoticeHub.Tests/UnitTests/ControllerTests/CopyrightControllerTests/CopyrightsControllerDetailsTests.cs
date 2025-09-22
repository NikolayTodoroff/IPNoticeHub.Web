using FluentAssertions;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;


namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    /// <summary>
    /// Section: CopyrightsController – Details
    ///  - If no user is authenticated, returns Forbid.
    ///  - If the service cannot find the requested item, returns NotFound.
    ///  - If successful, returns a View with the details.
    /// </summary>
    [TestFixture]
    public class CopyrightsControllerDetailsTests
    {
        [Test]
        public async Task Details_Success_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            var copyrightDetailsDTO = new CopyrightDetailsDTO { PublicId = id, RegistrationNumber = "TX-9", TypeOfWork = "Literary", Title = "T", Owner = "O" };

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync("u1", id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(copyrightDetailsDTO);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var detailsActionResult = await controller.Details(id);

            var detailsView = detailsActionResult.Should().BeOfType<ViewResult>().Subject;
            detailsView.Model.Should().Be(copyrightDetailsDTO);
        }

        [Test]
        public async Task Details_NoUser_ReturnsForbid()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: null);

            var detailsActionResult = await controller.Details(Guid.NewGuid());

            detailsActionResult.Should().BeOfType<ForbidResult>();
            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            var copyrightService = new Mock<ICopyrightService>();
            copyrightService.Setup(s => s.GetDetailsAsync("u1", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((CopyrightDetailsDTO?)null);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var detailsActionResult = await controller.Details(Guid.NewGuid());

            detailsActionResult.Should().BeOfType<NotFoundResult>();
        }
    }
}
