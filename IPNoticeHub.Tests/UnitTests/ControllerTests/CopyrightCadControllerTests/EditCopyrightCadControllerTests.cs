using FluentAssertions;
using IPNoticeHub.Application.DTOs.DocumentLibraryDTOs;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public class EditCopyrightCadControllerTests :BaseCopyrightCadControllerTests
    {
        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = controller.CeaseDesistEdit(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public void CeaseDesistEdit_Get_ShouldReturnView_WithViewModel_WhenUserIdExists()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work"
            };

            var result = controller.CeaseDesistEdit(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().BeSameAs(viewModel);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var vm = new CeaseDesistViewModel { PublicId = Guid.NewGuid() };

            var result = await controller.CeaseDesistEdit(vm, command: "save");

            result.Should().BeOfType<ForbidResult>();

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenModelStateInvalid()
        {
            var viewModel = new CeaseDesistViewModel { PublicId = Guid.NewGuid() };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = await controller.CeaseDesistEdit(viewModel, command: "save");

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistEdit");
            viewResult.Model.Should().BeSameAs(viewModel);

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    It.IsAny<string>(), 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldSaveDocument_SetTempData_AndRedirectToCeaseDesistEdit_WhenCommandIsSave()
        {
            var viewModel = new CeaseDesistViewModel
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

            var result = await controller.CeaseDesistEdit(viewModel, command: "save");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be(nameof(CopyrightCadController.CeaseDesistEdit));

            controller.TempData.ContainsKey("SuccessMessage").Should().BeTrue();
            controller.TempData["SuccessMessage"]!.ToString()
                .Should().Contain("successfully saved");

            documentLibraryService.Verify(
                s => s.SaveDocumentAsync(
                    TestUserId, 
                    It.IsAny<DocumentCreateDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CeaseDesistEdit_Post_ShouldRedirectToCopyrightsMyCollection_WhenCommandIsDone()
        {
            var viewModel = 
                new CeaseDesistViewModel { PublicId = Guid.NewGuid() };

            var result = 
                await controller.CeaseDesistEdit(
                    viewModel, 
                    command: "done");

            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)result;

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
        public async Task CeaseDesistEdit_Post_ShouldReturnView_WhenCommandIsUnknown()
        {
            var viewModel = 
                new CeaseDesistViewModel { PublicId = Guid.NewGuid() };

            var result = await controller.CeaseDesistEdit(viewModel, command: "wat");

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistEdit");
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
