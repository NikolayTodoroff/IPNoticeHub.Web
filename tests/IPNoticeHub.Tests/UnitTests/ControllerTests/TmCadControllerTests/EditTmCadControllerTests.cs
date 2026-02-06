using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCadControllerTests
{
    public class EditTmCadControllerTests : TmCadControllerBase
    {
        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnView_WithViewModel()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = controller.CeaseDesistEdit(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistEdit(viewModel);
            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldSaveDocument_WhenCommandIsSave()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                SenderName = "John Doe"
            };

            documentLibraryService.Setup(
                s => s.SaveDocumentAsync(
                    UserId,
                    It.IsAny<DocumentCreateDto>(),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(0);

            var result = 
                await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;

            redirectResult!.ActionName.Should().Be(nameof(controller.CeaseDesistEdit));

            controller.TempData["SuccessMessage"].Should().Be(
                "Your Cease & Desist letter was successfully saved to your library.");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    UserId,
                    It.IsAny<DocumentCreateDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldRedirectToMyCollection_WhenCommandIsDone()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = 
                await controller.CeaseDesistEdit(viewModel, "done");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;

            redirectResult!.ActionName.Should().Be("MyCollection");
            redirectResult.ControllerName.Should().Be("Trademarks");
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenCommandIsUnknown()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result =
                await controller.CeaseDesistEdit(viewModel, "unknown");

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = 
                await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = 
                await controller.CeaseDesistEdit(viewModel, "save");

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldNotSaveDocument_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");
            await controller.CeaseDesistEdit(viewModel, "save");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<DocumentCreateDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
