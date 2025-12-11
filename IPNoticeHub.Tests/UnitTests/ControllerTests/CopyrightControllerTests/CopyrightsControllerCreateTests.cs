using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Models.Copyrights;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.StatusMessages.CopyrightStatusMessages;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class CopyrightsControllerCreateTests
    {
        [Test]
        public void Create_Get_ReturnsViewWithEmptyDto()
        {
            var controller = 
                TestCopyrightControllerFactory.CreateController(
                Mock.Of<ICopyrightService>());

            var createActionResult = controller.Create();

            var viewMode = createActionResult.Should().
                BeOfType<ViewResult>().Subject;

            viewMode.Model.Should().
                BeOfType<CopyrightCreateViewModel>();
        }

        [Test]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDto()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object);

            var dto = new CopyrightCreateViewModel{};

            controller.ModelState.AddModelError("Title", "Required");

            var createActionResult = await controller.Create(dto);

            var viewMode = createActionResult.Should().
                BeOfType<ViewResult>().Subject;

            viewMode.Model.Should().
                Be(dto);

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_WithNoUser_ReturnsForbid()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: null);

            var dto = new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title", 
                Owner = "Owner" };

            var createActionResult = await controller.Create(dto);

            createActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_Success_LocalReturnUrl_RedirectsThere_AndSetsTempData()
        {
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(
                s => s.CreateAsync(
                    "u1", 
                It.IsAny<CopyrightCreateDto>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(Guid.NewGuid());

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, userId: "u1");

            var createCopyrightDTO = 
                new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-2123456", 
                WorkType = CopyrightWorkType.VisualArts,
                Title = "New Image",
                Owner = "New Artist" };

            var createActionResult = await controller.Create(
                createCopyrightDTO, 
                returnUrl: "/back");

            var redirectResult = createActionResult.Should().
                BeOfType<RedirectResult>().Subject;

            redirectResult.Url.Should().Be("/back");

            controller.TempData["SuccessMessage"].Should().
                Be(CopyrightAddedMessage);

            copyrightService.Verify(
                s => s.CreateAsync(
                    "u1", 
                It.IsAny<CopyrightCreateDto>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task CreatePostSuccess_NoReturnUrl_RedirectsToDetails_WithCorrectIdAndTempData()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.CreateAsync(
                "u1", 
                It.IsAny<CopyrightCreateDto>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(id);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var dto = new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-3",
                WorkType = CopyrightWorkType.Literary,
                Title = "New Title",
                Owner = "New Owner" };

            var createActionResult = await controller.Create(
                dto, 
                returnUrl: null);

            var redirect = createActionResult.Should().
                BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().
                Be(nameof(CopyrightsController.Details));

            redirect.RouteValues!["id"].Should().
                Be(id);

            controller.TempData["SuccessMessage"].Should().
                Be(CopyrightAddedMessage);
        }
    }
}
