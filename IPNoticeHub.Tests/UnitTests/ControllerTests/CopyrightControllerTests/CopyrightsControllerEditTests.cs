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
    public class CopyrightsControllerEditTests
    {
        [Test]
        public async Task Get_Edit_WhenDetailsNull_ReturnsNotFound()
        {
            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync((CopyrightDetailsDto?) null);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var editActionResult = 
                await controller.Edit(Guid.NewGuid());

            editActionResult.Should().
                BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Get_Edit_WhenDetailsPresent_MapsTypeOfWorkString_ToEnumAndOtherWorkType()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(
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
                copyrightService.Object, 
                userId: "u1");

            var editActionResult = await controller.Edit(id);

            var editView = editActionResult.Should().
                BeOfType<ViewResult>().Subject;

            var editViewModel = editView.Model.Should().
                BeOfType<CopyrightEditViewModel>().Subject;

            editViewModel.PublicId.Should().
                Be(id);

            editViewModel.WorkType.Should().
                Be(CopyrightWorkType.Other);

            editViewModel.OtherWorkType.Should().
                Be("Custom Type Of Work");
        }

        [Test]
        public async Task Post_Edit_WhenOtherWorkTypeMissing_AddsModelError_AndReturnsView()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Other,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(
                id, 
                editViewModel, 
                returnUrl: null);

            var editView = editActionResult.Should().
                BeOfType<ViewResult>().Subject;

            controller.ModelState.ContainsKey(
                nameof(CopyrightEditViewModel.OtherWorkType)).Should().
                BeTrue();

            editView.Model.Should().
                Be(editViewModel);

            copyrightService.Verify(
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

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(false);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(
                id, 
                editViewModel, 
                returnUrl: null);

            var editView = editActionResult.Should().
                BeOfType<ViewResult>().Subject;

            editView.Model.Should().
                Be(editViewModel);

            controller.ModelState.IsValid.Should().
                BeFalse();
        }

        [Test]
        public async Task PostEditOnSuccess_WithLocalReturnUrl_RedirectsToSpecifiedUrl()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(
                id, 
                editViewModel, 
                returnUrl: "/back");

            var redirectResult = editActionResult.Should().
                BeOfType<RedirectResult>().Subject;

            redirectResult.Url.Should().
                Be("/back");

            controller.TempData["SuccessMessage"].Should().
                Be(CopyrightUpdatesMessage);
        }

        [Test]
        public async Task Post_Edit_OnSuccess_NoReturnUrl_RedirectsToDetailsPage()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync(
                "u1", 
                id, 
                It.IsAny<CopyrightEditDto>(), 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(true);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(
                id, 
                editViewModel, 
                returnUrl: null);

            var redirectResult = editActionResult.Should().
                BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().
                Be(nameof(CopyrightsController.Details));

            redirectResult.RouteValues!["id"].Should().
                Be(id);
        }

        [Test]
        public async Task Get_Edit_WhenNoUser_ReturnsForbid()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service: copyrightService.Object,
                userId: null,
                includeTempData: false,
                includeUrlHelper: false);

            var editActionResult = 
                await controller.Edit(Guid.NewGuid());

            editActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Post_Edit_WhenNoUser_ReturnsForbid()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service: copyrightService.Object,
                userId: null,
                includeTempData: true,
                includeUrlHelper: true);

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = Guid.NewGuid(),
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(
                editViewModel.PublicId, 
                editViewModel, 
                returnUrl: null,
                cancellationToken: CancellationToken.None);

            editActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Get_Edit_MapsKnownEnumString_ToThatEnum_WithoutOtherText()
        {
            var id = Guid.NewGuid();
            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(
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
                copyrightService.Object, 
                userId: "u1");

            var editActionResult = await controller.Edit(id);

            var view = editActionResult.Should().
                BeOfType<ViewResult>().Subject;

            var editViewModel = view.Model.Should().
                BeOfType<CopyrightEditViewModel>().Subject;

            editViewModel.WorkType.Should().
                Be(CopyrightWorkType.VisualArts);

            editViewModel.OtherWorkType.Should().
                BeNull();
        }

        [Test]
        public async Task Get_Edit_MapsEmptyStoredString_ToOther_WithNullOtherText()
        {
            var id = Guid.NewGuid();

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(
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
                copyrightService.Object, 
                userId: "u1");

            var editActionResult = await controller.Edit(id);

            var editView = editActionResult.Should().
                BeOfType<ViewResult>().Subject;

            var editViewModel = editView.Model.Should().
                BeOfType<CopyrightEditViewModel>().Subject;

            editViewModel.WorkType.Should().
                Be(CopyrightWorkType.Other);

            editViewModel.OtherWorkType.Should().
                BeNull();
        }
    }
}
