using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
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
    /// <summary>
    /// Section: CopyrightsController – Create
    /// - GET Create returns view with empty DTO.
    /// - POST invalid ModelState returns same view with DTO.
    /// - POST no user returns Forbid.
    /// - POST success returns calls service, sets TempData, redirects to local returnUrl.
    /// - POST success with no/invalid returnUrl return redirects to Details(publicId).
    /// </summary>
    /// [TestFixture]
    public class CopyrightsControllerCreateTests
    {
        [Test]
        public void Create_Get_ReturnsViewWithEmptyDto()
        {
            var controller = TestCopyrightControllerFactory.CreateController(Mock.Of<ICopyrightService>());

            var createActionResult = controller.Create();

            var createView = createActionResult.Should().BeOfType<ViewResult>().Subject;
            createView.Model.Should().BeOfType<CopyrightCreateDTO>();
        }

        [Test]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDto()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object);

            var createCopyrightDTO = new CopyrightCreateDTO
            {
                // leaving the required fields empty should trigger model error
            };

            controller.ModelState.AddModelError("Title", "Required");

            var createActionResult = await controller.Create(createCopyrightDTO);

            var createView = createActionResult.Should().BeOfType<ViewResult>().Subject;
            createView.Model.Should().Be(createCopyrightDTO);
            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_WithNoUser_ReturnsForbid()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: null);

            var createCopyrightDTO = new CopyrightCreateDTO { 
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title", 
                Owner = "Owner" };

            var createActionResult = await controller.Create(createCopyrightDTO);

            createActionResult.Should().BeOfType<ForbidResult>();
            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_Success_LocalReturnUrl_RedirectsThere_AndSetsTempData()
        {
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.CreateAsync("u1", It.IsAny<CopyrightCreateDTO>(), It.IsAny<CancellationToken>())).
               ReturnsAsync(Guid.NewGuid());

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var createCopyrightDTO = new CopyrightCreateDTO { 
                RegistrationNumber = "TX-2123456", 
                WorkType = CopyrightWorkType.VisualArts,
                Title = "New Image",
                Owner = "New Artist" };

            var createActionResult = await controller.Create(createCopyrightDTO, returnUrl: "/back");


            var redirectResult = createActionResult.Should().BeOfType<RedirectResult>().Subject;
            redirectResult.Url.Should().Be("/back");
            controller.TempData["StatusMessage"].Should().Be(CopyrightAddedMessage);

            copyrightService.Verify(s => s.CreateAsync("u1", It.IsAny<CopyrightCreateDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Create_Post_Success_NoReturnUrl_RedirectsToDetails_WithCorrectIdAndTempData()
        {
            var createdId = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.CreateAsync("u1", It.IsAny<CopyrightCreateDTO>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(createdId);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var createCopyrightDTO = new CopyrightCreateDTO { 
                RegistrationNumber = "TX-3",
                WorkType = CopyrightWorkType.Literary,
                Title = "New Title",
                Owner = "New Owner" };

            var createActionResult = await controller.Create(createCopyrightDTO, returnUrl: null);

            var redirect = createActionResult.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be(nameof(CopyrightsController.Details));

            redirect.RouteValues!["id"].Should().Be(createdId);
            controller.TempData["StatusMessage"].Should().Be(CopyrightAddedMessage);
        }
    }
}
