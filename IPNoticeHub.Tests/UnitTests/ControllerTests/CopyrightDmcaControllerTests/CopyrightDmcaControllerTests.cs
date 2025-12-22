using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightDmcaControllerTests
{
    public class CopyrightDmcaControllerTests : BaseCopyrightDmcaControllerTests
    {
        [Test]
        public async Task Dmca_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.Dmca(Guid.NewGuid());

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task Dmca_Get_ShouldReturnNotFound_WhenDetailsDtoIsNull()
        {
            var publicId = Guid.NewGuid();

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    UserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((CopyrightDetailsDto?)null);

            var result = await controller.Dmca(publicId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Dmca_Get_ShouldReturnView_WithViewModel_AndTemplateBody()
        {
            var publicId = Guid.NewGuid();

            var dto = new CopyrightDetailsDto
            {
                PublicId = publicId,
                Title = "Test Work",
                RegistrationNumber = "REG-123"
            };

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    UserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(dto);

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey(CopyrightDmcaKey)).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.Dmca,
                    CopyrightDmcaKey,
                    "DMCA - General",
                    "DMCA TEMPLATE BODY"));

            var actionResult = await controller.Dmca(publicId);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.Model.Should().BeOfType<DmcaViewModel>();
            var viewModel = (DmcaViewModel)viewResult.Model!;

            viewModel.PublicId.Should().Be(publicId);
            viewModel.BodyTemplate.Should().Be("DMCA TEMPLATE BODY");

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey(CopyrightDmcaKey), 
                Times.Once);
        }

        [Test]
        public async Task Dmca_Post_ShouldReturnView_WhenModelStateInvalid()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new DmcaViewModel { PublicId = publicId };

            controller.ModelState.AddModelError(
                "SenderName", 
                "Required");

            var result = await controller.Dmca(publicId, viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.Model.Should().BeSameAs(viewModel);

            copyrightService.Verify(
                s => s.GetDetailsAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Dmca_Post_ShouldReturnForbid_WhenUserIdMissing_AndModelStateValid()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var publicId = Guid.NewGuid();
            var viewModel = new DmcaViewModel { PublicId = publicId };

            var result = await controller.Dmca(publicId, viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task Dmca_Post_ShouldReturnNotFound_WhenDetailsDtoIsNull()
        {
            var publicId = Guid.NewGuid();
            var vm = new DmcaViewModel { PublicId = publicId };

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    UserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((CopyrightDetailsDto?)null);

            var result = await controller.Dmca(publicId, vm);

            result.Should().BeOfType<NotFoundResult>();

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Dmca_Post_ShouldReturnPdfFile_WhenHappyPath()
        {
            var publicId = Guid.NewGuid();

            var dto = new CopyrightDetailsDto
            {
                PublicId = publicId,
                Title = "My Copyright Work",
                RegistrationNumber = "REG-999"
            };

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    UserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(dto);

            var viewModel = new DmcaViewModel
            {
                PublicId = publicId,
                WorkTitle = "User Title (will be overwritten)",
                BodyTemplate = "Final body"
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4 };

            pdfService.Setup(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(pdfBytes);

            var actionResult = await controller.Dmca(
                publicId, 
                viewModel);

            actionResult.Should().BeOfType<FileContentResult>();
            var file = (FileContentResult)actionResult;

            file.ContentType.Should().Be("application/pdf");
            file.FileContents.Should().BeEquivalentTo(pdfBytes);

            file.FileDownloadName.Should().Contain($"DMCA-{dto.Title}-");
            file.FileDownloadName.Should().EndWith(".pdf");

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
