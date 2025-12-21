using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightDmcaControllerTests
{
    public class PreviewCopyrightDmcaControllerTests : BaseCopyrightDmcaControllerTests
    {
        [Test]
        public async Task DmcaPreview_Get_ShouldReturnForbid_WhenUserIdMissing()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Some body"
            };

            var actionResult = await controller.DmcaPreview(viewModel);

            actionResult.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task DmcaPreview_Get_ShouldReplaceTemplate_WhenBodyTemplateIsNullOrWhitespace()
        {
            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "   "
            };

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey("DMCA-Copyright")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.Dmca,
                    "DMCA-Copyright",
                    "DMCA - Copyright",
                    "TEMPLATE"));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("REPLACED");

            var actionResult = await controller.DmcaPreview(viewModel);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaPreview");
            viewResult.Model.Should().BeSameAs(viewModel);
            viewModel.BodyTemplate.Should().Be("REPLACED");

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey("DMCA-Copyright"), 
                Times.Once);
            
            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>()), 
                Times.Once);
        }

        [Test]
        public async Task DmcaPreview_Get_ShouldReplaceTemplate_WhenBodyTemplateStillHasTokens()
        {
            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Hello {{WorkTitle}}"
            };

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey("DMCA-Copyright")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.Dmca,
                    "DMCA-Copyright",
                    "DMCA - Copyright",
                    "TEMPLATE"));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    "TEMPLATE", 
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("REPLACED");

            var actionResult = await controller.DmcaPreview(viewModel);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaPreview");
            viewModel.BodyTemplate.Should().Be("REPLACED");
        }

        [Test]
        public async Task DmcaPreview_Get_ShouldNotReplaceTemplate_WhenBodyTemplateIsAlreadyFinal()
        {
            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Already final body."
            };

            var actionResult = await controller.DmcaPreview(viewModel);

            actionResult.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)actionResult;

            viewResult.ViewName.Should().Be("DmcaPreview");
            viewResult.Model.Should().BeSameAs(viewModel);

            letterTemplateProvider.Verify(
                p => p.GetTemplateByKey(It.IsAny<string>()), 
                Times.Never);
            
            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(), 
                    It.IsAny<Dictionary<string, string>>()), 
                Times.Never);
        }

        //[Test]
        //public async Task DmcaPreview_Post_ShouldReturnDMCAView_WhenModelStateInvalid()
        //{
        //    var viewModel = new DmcaViewModel
        //    {
        //        PublicId = Guid.NewGuid()
        //    };

        //    controller.ModelState.AddModelError(
        //        "SenderName", 
        //        "Required");

        //    var actionResult = await controller.DmcaPreview(viewModel);

        //    actionResult.Should().BeOfType<ViewResult>();
        //    var viewResult = (ViewResult)actionResult;

        //    viewResult.ViewName.Should().Be("DMCA");
        //    viewResult.Model.Should().BeSameAs(viewModel);

        //    copyrightService.Verify(
        //        s => s.GetDetailsAsync(
        //            It.IsAny<string>(), 
        //            It.IsAny<Guid>(), 
        //            It.IsAny<CancellationToken>()),
        //        Times.Never);

        //    letterTemplateProvider.Verify(
        //        p => p.GetTemplateByKey(It.IsAny<string>()), 
        //        Times.Never);

        //    templateReplacer.Verify(
        //        r => r.ReplaceTemplate(
        //            It.IsAny<string>(), 
        //            It.IsAny<Dictionary<string, string>>()), 
        //        Times.Never);
        //}

        [Test]
        public async Task DmcaPreview_Post_ShouldReturnForbid_WhenUserIdMissing_AndModelStateValid()
        {
            controller.ControllerContext.HttpContext!.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var viewModel = new DmcaViewModel
            {
                PublicId = Guid.NewGuid(),
                BodyTemplate = "Final body"
            };

            var actionResult = await controller.DmcaPreview(viewModel);

            actionResult.Should().BeOfType<ForbidResult>();

            copyrightService.Verify(
                s => s.GetDetailsAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Guid>(), 
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        //[Test]
        //public async Task DmcaPreview_Post_ShouldFillBlanksFromDto_WhenDtoExists()
        //{
        //    var publicId = Guid.NewGuid();

        //    var viewModel = new DmcaViewModel
        //    {
        //        PublicId = publicId,
        //        WorkTitle = "",
        //        BodyTemplate = "Final body"
        //    };

        //    var dto = new CopyrightDetailsDto
        //    {
        //        PublicId = publicId,
        //        Title = "DB Title",
        //        RegistrationNumber = "DB-REG"
        //    };

        //    copyrightService.Setup(
        //        s => s.GetDetailsAsync(
        //            TestUserId, 
        //            publicId, 
        //            It.IsAny<CancellationToken>())).
        //            ReturnsAsync(dto);

        //    var actionResult = await controller.DmcaPreview(viewModel);

        //    actionResult.Should().BeOfType<RedirectToActionResult>();
        //    var redirect = (RedirectToActionResult)actionResult;

        //    redirect.ActionName.Should().Be(nameof(CopyrightDmcaController.DmcaPreview));

        //    viewModel.WorkTitle.Should().Be("DB Title");

        //    letterTemplateProvider.Verify(
        //        p => p.GetTemplateByKey(It.IsAny<string>()), 
        //        Times.Never);
            
        //    templateReplacer.Verify(
        //        r => r.ReplaceTemplate(
        //            It.IsAny<string>(), 
        //            It.IsAny<Dictionary<string, string>>()), 
        //        Times.Never);
        //}

        //[Test]
        //public async Task DmcaPreview_Post_ShouldReplaceTemplate_AndRedirect_WhenBodyTemplateHasTokens()
        //{
        //    var publicId = Guid.NewGuid();

        //    var viewModel = new DmcaViewModel
        //    {
        //        PublicId = publicId,
        //        WorkTitle = "User Title",
        //        BodyTemplate = "Hello {{WorkTitle}}"
        //    };

        //    copyrightService.Setup(
        //        s => s.GetDetailsAsync(
        //            TestUserId, 
        //            publicId, 
        //            It.IsAny<CancellationToken>())).
        //            ReturnsAsync((CopyrightDetailsDto?)null);

        //    letterTemplateProvider.Setup(
        //        p => p.GetTemplateByKey("DMCA-General")).
        //        Returns(new LetterTemplatePreset(
        //            LetterTemplateType.Dmca,
        //            "DMCA-General",
        //            "DMCA - General",
        //            "TEMPLATE"));

        //    templateReplacer.Setup(
        //        r => r.ReplaceTemplate(
        //            "TEMPLATE", 
        //            It.IsAny<Dictionary<string, string>>())).
        //            Returns("REPLACED");

        //    var actionResult = await controller.DmcaPreview(viewModel);

        //    actionResult.Should().BeOfType<RedirectToActionResult>();
        //    var redirect = (RedirectToActionResult)actionResult;

        //    redirect.ActionName.Should().Be(nameof(CopyrightDmcaController.DmcaPreview));
        //    viewModel.BodyTemplate.Should().Be("REPLACED");

        //    letterTemplateProvider.Verify(
        //        p => p.GetTemplateByKey("DMCA-General"), 
        //        Times.Once);
            
        //    templateReplacer.Verify(
        //        r => r.ReplaceTemplate(
        //            "TEMPLATE", 
        //            It.IsAny<Dictionary<string, string>>()), 
        //        Times.Once);
        //}
    }
}
