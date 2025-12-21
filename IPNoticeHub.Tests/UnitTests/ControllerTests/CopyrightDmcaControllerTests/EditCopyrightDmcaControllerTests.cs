using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightDmcaControllerTests
{
    public class EditCopyrightDmcaControllerTests : BaseCopyrightDmcaControllerTests
    {
        [Test]
        public void DmcaEdit_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var actionResult = controller.DmcaEdit(viewModel);

            actionResult.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public void DmcaEdit_Get_ShouldReturnView_WithViewModel_WhenUserIdExists()
        {
            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var actionResult = controller.DmcaEdit(viewModel);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaEdit");
            viewResult.Model.Should().BeSameAs(viewModel);
        }

        [Test]
        public async Task DmcaEdit_Post_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new DmcaViewModel { PublicId = Guid.NewGuid() };

            var actionResult = await controller.DmcaEdit(viewModel, command: "save");

            actionResult.Should().BeOfType<ForbidResult>();

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DmcaEdit_Post_ShouldReturnView_WhenModelStateInvalid()
        {
            var viewModel = new DmcaViewModel { PublicId = Guid.NewGuid() };

            controller.ModelState.AddModelError("SenderName", "Required");

            var actionResult = await controller.DmcaEdit(
                viewModel, 
                command: "save");

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaEdit");
            viewResult.Model.Should().BeSameAs(viewModel);

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DmcaEdit_Post_ShouldSaveDocument_SetTempData_AndRedirectToDmcaEdit_WhenCommandIsSave()
        {
            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "Final body"
            };

            documentLibraryService.Setup(
                s => s.SaveDocumentAsync(
                    TestUserId, 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>())).
                    Returns(Task.FromResult(0));

            var actionResult = await controller.DmcaEdit(
                viewModel, 
                command: "save");

            actionResult.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)actionResult;

            redirect.ActionName.Should().Be(nameof(CopyrightDmcaController.DmcaEdit));

            controller.TempData.ContainsKey("SuccessMessage").Should().BeTrue();

            controller.TempData["SuccessMessage"]!.ToString().
                Should().Contain("successfully saved");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    TestUserId, 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DmcaEdit_Post_ShouldRedirectToCopyrightsMyCollection_WhenCommandIsDone()
        {
            var viewModel = new DmcaViewModel { PublicId = Guid.NewGuid() };

            var actionResult = await controller.DmcaEdit(
                viewModel, 
                command: "done");

            actionResult.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)actionResult;

            redirect.ActionName.Should().Be("MyCollection");
            redirect.ControllerName.Should().Be("Copyrights");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DmcaEdit_Post_ShouldReturnView_WhenCommandIsUnknown()
        {
            var viewModel = new DmcaViewModel { PublicId = Guid.NewGuid() };

            var actionResult = await controller.DmcaEdit(
                viewModel, 
                command: "unknown");

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaEdit");
            viewResult.Model.Should().BeSameAs(viewModel);

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
