using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
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
    public class CreateCopyrightsControllerTests
    {
        [Test]
        public void Create_Get_ReturnsViewWithEmptyDto()
        {
            var controller = 
                TestCopyrightControllerFactory.CreateController(
                Mock.Of<ICopyrightService>());

            var actionResult = controller.Create();

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().BeOfType<CopyrightCreateViewModel>();
        }

        [Test]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDto()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object);

            var viewModel = new CopyrightCreateViewModel{};

            controller.ModelState.AddModelError("Title", "Required");

            var actionResult = await controller.Create(viewModel);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().Be(viewModel);
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_WithNoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: null);

            var viewModel = new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title", 
                Owner = "Owner" };

            var actionResult = await controller.Create(viewModel);
            actionResult.Should().BeOfType<ForbidResult>();
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Create_Post_Success_LocalReturnUrl_RedirectsThere_AndSetsTempData()
        {
            var service = new Mock<ICopyrightService>();

            service.Setup(
                s => s.CreateAsync(
                    "u1", 
                It.IsAny<CopyrightCreateDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(Guid.NewGuid());

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, userId: "u1");

            var createCopyrightDTO = 
                new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-2123456", 
                WorkType = CopyrightWorkType.VisualArts,
                Title = "New Image",
                Owner = "New Artist" };

            var actionResult = await controller.Create(
                createCopyrightDTO, 
                returnUrl: "/back");

            var redirect = 
                actionResult.Should().BeOfType<RedirectResult>().Subject;

            redirect.Url.Should().Be("/back");
            controller.TempData["SuccessMessage"].Should().Be(CopyrightAddedMessage);

            service.Verify(
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

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(s => s.CreateAsync(
                "u1", 
                It.IsAny<CopyrightCreateDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(id);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var viewModel = new CopyrightCreateViewModel { 
                RegistrationNumber = "TX-3",
                WorkType = CopyrightWorkType.Literary,
                Title = "New Title",
                Owner = "New Owner" };

            var actionResult = await controller.Create(
                viewModel, 
                returnUrl: null);

            var redirect = 
                actionResult.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().
                Be(nameof(CopyrightsController.Details));

            redirect.RouteValues!["id"].Should().Be(id);

            controller.TempData[
                "SuccessMessage"].Should().Be(CopyrightAddedMessage);
        }
    }
}
