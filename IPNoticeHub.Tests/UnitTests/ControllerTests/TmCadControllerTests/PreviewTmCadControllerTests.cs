using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCadControllerTests
{
    public class PreviewTmCadControllerTests : BaseTmCadControllerTests
    {
        [Test]
        public void CeaseDesistPreview_Get_ShouldReplaceTokens_WhenBodyContainsPlaceholders()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "Dear {{RecipientName}}, regarding {{WorkTitle}}"
            };

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    "Dear {{RecipientName}}, regarding {{WorkTitle}}"));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("Replaced Template");

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.ViewName.Should().Be("CeaseDesistPreview");

            var model = viewResult.Model as CeaseDesistViewModel;
            model!.BodyTemplate.Should().Be("Replaced Template");

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()),
                Times.Once);
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldNotReplaceTokens_WhenBodyHasNoPlaceholders()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "This is a complete template with no placeholders"
            };

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as CeaseDesistViewModel;

            model!.BodyTemplate.Should().Be(
                "This is a complete template with no placeholders");

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()),
                Times.Never);
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldReplaceTokens_WhenBodyIsEmpty()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = string.Empty
            };

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    ""));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("Replaced Template");

            var result = controller.CeaseDesistPreview(viewModel);

            var viewResult = result as ViewResult;
            var model = viewResult!.Model as CeaseDesistViewModel;

            model!.BodyTemplate.Should().Be("Replaced Template");
        }

        [Test]
        public void CeaseDesistPreview_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldRedirectToPreview_WhenModelStateIsValid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Test Work",
                BodyTemplate = "{{WorkTitle}}"
            };

            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    viewModel.PublicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(trademarkDto);

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey("CND-Trademark")).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    "CND-Trademark",
                    "Trademark Cease & Desist",
                    "{{WorkTitle}}"));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>())).
                    Returns("Replaced Template");

            var result = await controller.CeaseDesistPreview(
                viewModel, 
                CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = result as RedirectToActionResult;
            redirectResult!.Should().BeOfType<RedirectToActionResult>();
            redirectResult.ActionName.Should().Be(nameof(controller.CeaseDesistPreview));
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid()
            };

            controller.ModelState.AddModelError("SenderName", "Required");

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;

            viewResult!.ViewName.Should().Be("CeaseDesistPreview");
            viewResult.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesistPreview_Post_ShouldFetchTrademarkDetails_WhenAvailable()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = Guid.NewGuid(),
                WorkTitle = "Original Work",
                BodyTemplate = "Body {{Wordmark}}"
            };

            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Updated Trademark",
                RegistrationNumber = "REG999"
            };

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    viewModel.PublicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(trademarkDto);

            var result =
                await controller.CeaseDesistPreview(viewModel, CancellationToken.None);

            trademarkSearchService.Verify(
                s => s.GetDetailsAsync(
                    viewModel.PublicId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
