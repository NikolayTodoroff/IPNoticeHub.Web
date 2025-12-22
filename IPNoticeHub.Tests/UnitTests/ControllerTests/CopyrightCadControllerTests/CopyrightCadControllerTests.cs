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

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public class CopyrightCadControllerTests : BaseCopyrightCadControllerTests
    {
        [Test]
        public async Task CeaseDesist_Get_ShouldReturnView_WithViewModel_WhenCopyrightExists()
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
                    TestUserId,
                    publicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(dto);

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey(CopyrightCeaseDesistKey)).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    CopyrightCeaseDesistKey,
                    "Copyright Cease & Desist",
                    "Test Template Body"));

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.Model.Should().BeOfType<CeaseDesistViewModel>();
            var viewModel = (CeaseDesistViewModel)viewResult.Model!;

            viewModel.PublicId.Should().Be(publicId);
            viewModel.WorkTitle.Should().Be("Test Work");
            viewModel.RegistrationNumber.Should().Be("REG-123");
            viewModel.BodyTemplate.Should().Be("Test Template Body");

            controller.ViewData["ShowAdditionalFacts"].Should().Be(true);
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnNotFound_WhenDetailsDtoIsNull()
        {
            var publicId = Guid.NewGuid();

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    TestUserId,
                    publicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((CopyrightDetailsDto?)null);

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            var publicId = Guid.NewGuid();

            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnFileContentResult_WhenModelStateIsValid()
        {
            var publicId = Guid.NewGuid();

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                RegistrationNumber = "REG-123",
                SenderName = "John Doe",
                RecipientName = "Jane Smith",
                BodyTemplate = "Body"
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            pdfService.Setup(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(pdfBytes);

            var result = await controller.CeaseDesist(
                publicId, 
                viewModel);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;

            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);
            fileResult.FileDownloadName.Should().Contain("CeaseDesist-Test Work-");
            fileResult.FileDownloadName.Should().EndWith(".pdf");
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var publicId = Guid.NewGuid();

            var viewModel = 
                new CeaseDesistViewModel { PublicId = publicId };

            controller.ModelState.AddModelError(
                "SenderName", 
                "Required");

            var result = await controller.CeaseDesist(
                publicId, 
                viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().Be(viewModel);

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldCallPdfServiceWithMappedInput()
        {
            var publicId = Guid.NewGuid();

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                RegistrationNumber = "REG-123",
                SenderName = "John Doe",
                SenderAddress = "1 Main St",
                SenderEmail = "john@example.com",
                RecipientName = "Jane Smith",
                RecipientAddress = "2 High St",
                RecipientEmail = "jane@example.com",
                InfringingUrl = "https://example.com/infringing",
                AdditionalFacts = "Additional facts",
                BodyTemplate = "Body"
            };

            LetterInputDto? captured = null;
            var pdfBytes = new byte[] { 9, 8, 7 };

            pdfService.Setup(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                    Callback<LetterInputDto, 
                    CancellationToken>((input, _) => captured = input).
                    ReturnsAsync(pdfBytes);

            await controller.CeaseDesist(publicId, viewModel);

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);

            captured.Should().NotBeNull();
            captured!.WorkTitle.Should().Be(viewModel.WorkTitle);
            captured.RegistrationNumber.Should().Be(viewModel.RegistrationNumber);
            captured.SenderName.Should().Be(viewModel.SenderName);
            captured.SenderAddress.Should().Be(viewModel.SenderAddress);
            captured.SenderEmail.Should().Be(viewModel.SenderEmail);
            captured.RecipientName.Should().Be(viewModel.RecipientName);
            captured.RecipientAddress.Should().Be(viewModel.RecipientAddress);
            captured.RecipientEmail.Should().Be(viewModel.RecipientEmail);
            captured.InfringingUrl.Should().Be(viewModel.InfringingUrl);
            captured.AdditionalFacts.Should().Be(viewModel.AdditionalFacts);
            captured.BodyTemplate.Should().Be(viewModel.BodyTemplate);
        }
    }
}
