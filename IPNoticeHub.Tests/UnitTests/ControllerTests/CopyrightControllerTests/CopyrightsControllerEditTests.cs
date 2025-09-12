using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.Copyrights;
using static IPNoticeHub.Common.ValidationConstants.StatusMessages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    /// <summary>
    /// Section: CopyrightsController – Edit (GET/POST)
    /// - GET Edit: Returns NotFound if the service returns null (missing or unlinked resource).
    /// - GET Edit: Maps the stored TypeOfWork string to an enum bucket and additional text.
    /// - POST Edit: Returns a view with a ModelState error if WorkType is "Other" and OtherWorkType is missing.
    /// - POST Edit: Returns a view with an error if the service operation fails.
    /// - POST Edit: On success, sets TempData and redirects to either a local returnUrl or the Details page.
    /// - GET Edit without user returns Forbid
    /// - POST Edit without user returns Forbid
    /// </summary>
    [TestFixture]
    public class CopyrightsControllerEditTests
    {
        [Test]
        public async Task Get_Edit_WhenDetailsNull_ReturnsNotFound()
        {
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync("u1", It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((CopyrightDetailsDTO?) null);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editActionResult = await controller.Edit(Guid.NewGuid());

            editActionResult.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Get_Edit_WhenDetailsPresent_MapsTypeOfWorkString_ToEnumAndOtherWorkType()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync("u1", id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CopyrightDetailsDTO
                   {
                       PublicId = id,
                       RegistrationNumber = "TX-1",
                       TypeOfWork = "Custom Type Of Work",
                       Title = "New Title",
                       Owner = "New Owner"
                   });

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editActionResult = await controller.Edit(id);
            var editView = editActionResult.Should().BeOfType<ViewResult>().Subject;

            var editViewModel = editView.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;

            editViewModel.PublicId.Should().Be(id);
            editViewModel.WorkType.Should().Be(CopyrightWorkType.Other);
            editViewModel.OtherWorkType.Should().Be("Custom Type Of Work");
        }

        [Test]
        public async Task Post_Edit_WhenOtherWorkTypeMissing_AddsModelError_AndReturnsView()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Other,
                // missing OtherWorkType
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(id, editViewModel, returnUrl: null);

            var editView = editActionResult.Should().BeOfType<ViewResult>().Subject;

            controller.ModelState.ContainsKey(nameof(CopyrightEditViewModel.OtherWorkType)).Should().BeTrue();
            editView.Model.Should().Be(editViewModel);

            copyrightService.Verify(s => s.EditAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CopyrightEditDTO>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Post_Edit_WhenCopyrightServiceReturnsFalse_ReturnsViewWithError()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync("u1", id, It.IsAny<CopyrightEditDTO>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(id, editViewModel, returnUrl: null);

            var editView = editActionResult.Should().BeOfType<ViewResult>().Subject;

            editView.Model.Should().Be(editViewModel);
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Test]
        public async Task Post_Edit_OnSuccess_WithLocalReturnUrl_RedirectsToSpecifiedUrl_AndSetsTempData()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync("u1", id, It.IsAny<CopyrightEditDTO>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(id, editViewModel, returnUrl: "/back");

            var redirectResult = editActionResult.Should().BeOfType<RedirectResult>().Subject;
            redirectResult.Url.Should().Be("/back");
            controller.TempData["StatusMessage"].Should().Be(CopyrightUpdatesMessage);
        }

        [Test]
        public async Task Post_Edit_OnSuccess_NoReturnUrl_RedirectsToDetailsPage()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.EditAsync("u1", id, It.IsAny<CopyrightEditDTO>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editViewModel = new CopyrightEditViewModel
            {
                PublicId = id,
                RegistrationNumber = "TX-1",
                WorkType = CopyrightWorkType.Literary,
                Title = "Title",
                Owner = "Owner"
            };

            var editActionResult = await controller.Edit(id, editViewModel, returnUrl: null);

            var redirectResult = editActionResult.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(CopyrightsController.Details));
            redirectResult.RouteValues!["id"].Should().Be(id);
        }

        [Test]
        public async Task Get_Edit_WhenNoUser_ReturnsForbid()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(
                service: copyrightService.Object,
                userId: null,
                includeTempData: false,
                includeUrlHelper: false);

            var editActionResult = await controller.Edit(Guid.NewGuid());

            editActionResult.Should().BeOfType<ForbidResult>();
            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Post_Edit_WhenNoUser_ReturnsForbid()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = TestCopyrightControllerFactory.CreateController(
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

            var editActionResult = await controller.Edit(editViewModel.PublicId, editViewModel, returnUrl: null, cancellationToken: CancellationToken.None);

            editActionResult.Should().BeOfType<ForbidResult>();
            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Get_Edit_MapsKnownEnumString_ToThatEnum_WithoutOtherText()
        {
            var id = Guid.NewGuid();
            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync("u1", id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CopyrightDetailsDTO
                   {
                       PublicId = id,
                       RegistrationNumber = "TX-KNOWN",
                       TypeOfWork = "VisualArts",
                       Title = "Title",
                       Owner = "Owner"
                   });

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editActionResult = await controller.Edit(id);
            var view = editActionResult.Should().BeOfType<ViewResult>().Subject;

            var editViewModel = view.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;

            editViewModel.WorkType.Should().Be(CopyrightWorkType.VisualArts);
            editViewModel.OtherWorkType.Should().BeNull();
        }

        [Test]
        public async Task Get_Edit_MapsEmptyStoredString_ToOther_WithNullOtherText()
        {
            var id = Guid.NewGuid();

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetDetailsAsync("u1", id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new CopyrightDetailsDTO
                   {
                       PublicId = id,
                       RegistrationNumber = "TX-EMPTY",
                       TypeOfWork = "",
                       Title = "Title",
                       Owner = "Owner"
                   });

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: "u1");

            var editActionResult = await controller.Edit(id);
            var editView = editActionResult.Should().BeOfType<ViewResult>().Subject;

            var editViewModel = editView.Model.Should().BeOfType<CopyrightEditViewModel>().Subject;
            editViewModel.WorkType.Should().Be(CopyrightWorkType.Other);
            editViewModel.OtherWorkType.Should().BeNull();
        }
    }
}
