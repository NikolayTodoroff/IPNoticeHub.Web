using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Models.Copyrights;
using static IPNoticeHub.Shared.Constants.StatusMessages.CopyrightStatusMessages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class EditCopyrightsControllerTests
    {
        [Test]
        public async Task Get_Edit_WhenDetailsNull_ReturnsNotFound()
        {
            var service = 
                new Mock<ICopyrightService>();

            service.Setup(s => s.GetDetailsAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((CopyrightDetailsDto?) null);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = await controller.Edit(Guid.NewGuid());
            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Get_Edit_WhenDetailsPresent_MapsTypeOfWorkString_ToEnumAndOtherWorkType()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetDetailsAsync(
                "u1", 
                id, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(new CopyrightDetailsDto
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                TypeOfWork = "Custom Type Of Work",
                Title = "New Title",
                Owner = "New Owner"
            });


            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = await controller.Edit(id);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            var viewModel = 
                viewResult.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;

            viewModel.PublicId.Should().Be(id);
            viewModel.WorkType.Should().Be(CopyrightWorkType.Other);
            viewModel.OtherWorkType.Should().Be("Custom Type Of Work");
        }

        [Test]
        public async Task Post_Edit_WhenOtherWorkTypeMissing_AddsModelError_AndReturnsView()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Other,
                Title = "Title",
                Owner = "Owner"
            };

            var actionResult = await controller.Edit(
                id, 
                viewModel, 
                returnUrl: null);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            controller.ModelState.ContainsKey(
                nameof(CopyrightEditViewModel.OtherWorkType)).Should().BeTrue();

            viewResult.Model.Should().Be(viewModel);

            service.Verify(
                s => s.EditAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Guid>(),
                    It.IsAny<CopyrightEditDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task PostEdit_WhenCopyrightServiceReturnsFalse_ReturnsViewWithError()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var actionResult = await controller.Edit(
                id, 
                viewModel, 
                returnUrl: null);

            var editView = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            editView.Model.Should().Be(viewModel);
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Test]
        public async Task PostEditOnSuccess_WithLocalReturnUrl_RedirectsToSpecifiedUrl()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var actionResult = await controller.Edit(
                id, 
                viewModel, 
                returnUrl: "/back");

            var redirectResult = 
                actionResult.Should().BeOfType<RedirectResult>().Subject;

            redirectResult.Url.Should().Be("/back");

            controller.TempData[
                "SuccessMessage"].Should().Be(CopyrightUpdatesMessage);
        }

        [Test]
        public async Task Post_Edit_OnSuccess_NoReturnUrl_RedirectsToDetailsPage()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var actionResult = await controller.Edit(
                id, 
                viewModel, 
                returnUrl: null);

            var redirectResult = 
                actionResult.Should().BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().Be(nameof(CopyrightsController.Details));
            redirectResult.RouteValues!["id"].Should().Be(id);
        }

        [Test]
        public async Task Get_Edit_WhenNoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service: service.Object,
                userId: null,
                includeTempData: false,
                includeUrlHelper: false);

            var actionResult = 
                await controller.Edit(Guid.NewGuid());

            actionResult.Should().BeOfType<ForbidResult>();
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Post_Edit_WhenNoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service: service.Object,
                userId: null,
                includeTempData: true,
                includeUrlHelper: true);

            var viewModel = new CopyrightEditViewModel
            {
                PublicId = Guid.NewGuid(),
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var actionResult = await controller.Edit(
                viewModel.PublicId, 
                viewModel, 
                returnUrl: null,
                cancellationToken: CancellationToken.None);

            actionResult.Should().BeOfType<ForbidResult>();
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Get_Edit_MapsKnownEnumString_ToThatEnum_WithoutOtherText()
        {
            var id = Guid.NewGuid();
            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetDetailsAsync(
                "u1", 
                id, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(new CopyrightDetailsDto
            {
                PublicId = id,
                RegistrationNumber = "TX-KNOWN",
                TypeOfWork = "VisualArts",
                Title = "Title",
                Owner = "Owner"
                });

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = await controller.Edit(id);

            var view = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            var viewModel = 
                view.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;

            viewModel.WorkType.Should().Be(CopyrightWorkType.VisualArts);
            viewModel.OtherWorkType.Should().BeNull();
        }

        [Test]
        public async Task Get_Edit_MapsEmptyStoredString_ToOther_WithNullOtherText()
        {
            var id = Guid.NewGuid();

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetDetailsAsync(
                "u1", 
                id, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(new CopyrightDetailsDto
            {
                PublicId = id,
                RegistrationNumber = "TX-EMPTY",
                TypeOfWork = "",
                Title = "Title",
                Owner = "Owner"
                });

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = await controller.Edit(id);

            var editView = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            var viewModel = 
                editView.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;

            viewModel.WorkType.Should().Be(CopyrightWorkType.Other);
            viewModel.OtherWorkType.Should().BeNull();
        }
    }
}
