using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public class PreviewCopyrightCadControllerTests : BaseCopyrightCadControllerTests
    {
        [Test]
        public async Task CeaseDesistPreview_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Some body"
            };

            var result = await controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistPreview_Get_ShouldReplaceTemplate_WhenBodyTemplateIsNullOrWhitespace()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                RegistrationNumber = "REG-1",
                BodyTemplate = "   "
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Copyright")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Copyright",
                    "Copyright Cease & Desist",
                    "TEMPLATE"));

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("REPLACED");

            var result = await controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistPreview");
            viewResult.Model.Should().BeSameAs(viewModel);
            viewModel.BodyTemplate.Should().Be("REPLACED");

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey("CND-Copyright"), Times.Once);

            templateReplacer.Verify(
                r => r.ReplaceTemplate("TEMPLATE", 
                It.IsAny<Dictionary<string, string>>()), 
                Times.Once);
        }

        [Test]
        public async Task CeaseDesistPreview_Get_ShouldReplaceTemplate_WhenBodyTemplateStillHasTokens()
        {
            var vm = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "Hello {{WorkTitle}}"
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey("CND-Copyright")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Copyright",
                    "Copyright Cease & Desist",
                    "TEMPLATE"));

            templateReplacer.
                Setup(r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("REPLACED");

            var result = await controller.CeaseDesistPreview(vm);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistPreview");
            vm.BodyTemplate.Should().Be("REPLACED");
        }

        [Test]
        public async Task CeaseDesistPreview_Get_ShouldNotReplaceTemplate_WhenBodyTemplateIsAlreadyFinal()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Already final body text."
            };

            var result = await controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistPreview");
            viewResult.Model.Should().BeSameAs(viewModel);

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey(
                    It.IsAny<string>()), 
                Times.Never);
            
            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(), 
                    It.IsAny<Dictionary<string, string>>()), 
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnCeaseDesistView_WhenModelStateInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError(
                "SenderName", 
                "Required");

            var result = await controller.CeaseDesistPreview(viewModel, CancellationToken.None);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesist");
            viewResult.Model.Should().BeSameAs(viewModel);

            copyrightService.Verify(
                s => s.GetDetailsAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);

            letterTemplateProvider.Verify(p => p.GetTemplateByKey(
                It.IsAny<string>()), 
                Times.Never);

            templateReplacer.Verify(r => r.ReplaceTemplate(
                It.IsAny<string>(), 
                It.IsAny<Dictionary<string, string>>()), 
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnForbid_WhenUserIdMissing_AndModelStateValid()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var vm = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Final body"
            };

            var result = await controller.CeaseDesistPreview(vm);

            result.Should().BeOfType<ForbidResult>();

            copyrightService.Verify(
                s => s.GetDetailsAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldFillBlanksFromDto_WhenDtoExists()
        {
            var publicId = Guid.NewGuid();

            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "",
                RegistrationNumber = "   ",
                BodyTemplate = "Final body"
            };

            var dto = new CopyrightDetailsDto
            {
                PublicId = publicId,
                Title = "DB Title",
                RegistrationNumber = "DB-REG"
            };

            copyrightService.
                Setup(s => s.GetDetailsAsync(
                    TestUserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(dto);

            var result = await controller.CeaseDesistPreview(viewModel, CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be(nameof(controller.CeaseDesistPreview));

            viewModel.WorkTitle.Should().Be("DB Title");
            viewModel.RegistrationNumber.Should().Be("DB-REG");

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey(
                    It.IsAny<string>()), 
                Times.Never);

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(), 
                    It.IsAny<Dictionary<string, string>>()), 
                Times.Never);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReplaceTemplate_WhenBodyTemplateHasTokens()
        {
            var publicId = Guid.NewGuid();

            var vm = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "User Title",
                BodyTemplate = "Hello {{WorkTitle}}"
            };

            copyrightService.Setup(
                s => s.GetDetailsAsync(
                    TestUserId, 
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((CopyrightDetailsDto?)null);

            letterTemplateProvider
                .Setup(p => p.GetTemplateByKey("CND-Copyright"))
                .Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Copyright",
                    "Copyright Cease & Desist",
                    "TEMPLATE"));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("REPLACED");

            var result = await controller.CeaseDesistPreview(vm);

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.ViewName.Should().Be("CeaseDesistPreview");
            vm.BodyTemplate.Should().Be("REPLACED");

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey("CND-Copyright"), Times.Once);
            
            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>()), 
                Times.Once);
        }
    }
}
