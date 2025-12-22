using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCadControllerTests
{
    public class PreviewTmCadControllerTests : BaseTmCadControllerTests
    {
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
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    TrademarkCeaseDesistKey,
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
        public async Task CeaseDesistPreview_Post_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        
    }
}
