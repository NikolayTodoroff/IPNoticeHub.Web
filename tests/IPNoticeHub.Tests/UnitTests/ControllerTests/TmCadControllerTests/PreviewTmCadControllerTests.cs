using FluentAssertions;
using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Constants;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TmCadControllerTests
{
    public class PreviewTmCadControllerTests : TmCadControllerBase
    {
        [Test]
        public async Task PostTrademarkCadPreview_ShouldRedirectToPreview_WhenModelStateIsValid()
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
                    It.IsAny<IReadOnlyDictionary<string, string>>())).
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
        public async Task PostTrademarkCadPreview_ShouldReturnForbid_WhenUserIdMissing_AndModelIsStateValid()
        {
            var viewModel = new CeaseDesistViewModel();

            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesistPreview(viewModel);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task PostTrademarkCadPreview_ShouldSaveDraft_AndRedirectToPreview()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = PublicId,
                WorkTitle = "Some Work",
                SenderName = "Alice",
                BodyTemplate = "should be cleared"
            };

            var expectedDraftId = Guid.NewGuid();

            draftStore.Setup(
                s => s.SaveAsync(
                    It.Is<string>(u => u == UserId),
                    It.Is<string>(ks => ks == KeySpace),
                    It.Is<CeaseDesistDraftDto>(d =>
                        d.WorkTitle == viewModel.WorkTitle &&
                        d.SenderName == viewModel.SenderName),
                    It.Is<TimeSpan>(ttl => ttl == InputDraftConstants.UserInputDraftOptions.InputTtl),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(expectedDraftId);

            var actionResult = await controller.CeaseDesistPreview(
                viewModel,
                CancellationToken.None);

            var redirect =
                actionResult.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be(nameof(CopyrightCadController.CeaseDesistPreview));

            redirect.RouteValues.Should().ContainKey("publicId").
                WhoseValue.Should().Be(PublicId);

            redirect.RouteValues.Should().ContainKey("draftId").
                WhoseValue.Should().Be(expectedDraftId);

            draftStore.VerifyAll();

            trademarkSearchService.Verify(
                s => s.GetDetailsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            letterTemplateProvider.Verify(
                t => t.GetTemplateByKey(
                    It.IsAny<string>()),
                Times.Never);

            templateReplacer.Verify(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()),
                Times.Never);
        }

        [Test]
        public async Task PostTrademarkCadPreview_ShouldClearBodyTemplate_BeforeSaving()
        {
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = PublicId,
                BodyTemplate = "user-posted-body-should-not-survive",
                SenderName = "Alice"
            };

            draftStore.Setup(
                s => s.SaveAsync(
                    It.Is<string>(u => u == UserId),
                    It.Is<string>(ks => ks == KeySpace),
                    It.IsAny<CeaseDesistDraftDto>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(Guid.NewGuid());

            var result = await controller.CeaseDesistPreview(
                viewModel,
                CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            viewModel.BodyTemplate.Should().BeEmpty();
            draftStore.VerifyAll();
        }

        [Test]
        public async Task GetTrademarkCadPreview_ShouldReturnForbid_WhenUserMissing()
        {
            controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesistPreview(
                PublicId,
                Guid.NewGuid(),
                CancellationToken.None);

            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task GetTrademarkCadPreview_ShouldReturnForbid_WhenUserIdMissing()
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
        public async Task GetTrademarkCadPreview_ShouldRedirectToCeaseDesist_WhenDraftIdMissing()
        {
            var result = await controller.CeaseDesistPreview(
                PublicId,
                null, CancellationToken.None);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be("CeaseDesist");
            redirect.RouteValues!["publicId"].Should().Be(PublicId);
            controller.TempData.ContainsKey("PreviewInfo").Should().BeTrue();

            draftStore.VerifyNoOtherCalls();
            letterTemplateProvider.VerifyNoOtherCalls();
            templateReplacer.VerifyNoOtherCalls();
            trademarkSearchService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetTrademarkCadPreview_ShouldRedirect_WhenDraftMissingOrExpired()
        {
            var draftId = Guid.NewGuid();

            draftStore.Setup(
                d => d.GetAsync<CeaseDesistDraftDto>(
                    UserId,
                    draftId,
                    KeySpace,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((CeaseDesistDraftDto)null!);

            var result = await controller.CeaseDesistPreview(
                PublicId,
                draftId,
                CancellationToken.None);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be("CeaseDesist");
            redirect.RouteValues!["publicId"].Should().Be(PublicId);
            controller.TempData.ContainsKey("PreviewInfo").Should().BeTrue();

            draftStore.VerifyAll();
            letterTemplateProvider.VerifyNoOtherCalls();
            templateReplacer.VerifyNoOtherCalls();
            trademarkSearchService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetTrademarkCadPreview_ShouldRedirect_WhenDetailsMissing()
        {
            var draftId = Guid.NewGuid();
            var draft = new CeaseDesistDraftDto { SenderName = "Alice" };

            draftStore.Setup(
                d => d.GetAsync<CeaseDesistDraftDto>(
                    UserId,
                    draftId,
                    KeySpace,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(draft);

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    PublicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((TrademarkDetailsDto)null!);

            var result = await controller.CeaseDesistPreview(
                PublicId,
                draftId,
                CancellationToken.None);

            var redirect =
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be("CeaseDesist");
            redirect.RouteValues!["publicId"].Should().Be(PublicId);
            controller.TempData.ContainsKey("PreviewInfo").Should().BeTrue();

            draftStore.VerifyAll();
            trademarkSearchService.VerifyAll();
            letterTemplateProvider.VerifyNoOtherCalls();
            templateReplacer.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetTrademarkCadPreview_ShouldReturnView_WithComposedBody()
        {
            var draftId = Guid.NewGuid();

            var draft = new CeaseDesistDraftDto { SenderName = "Alice" };

            draftStore.Setup(
                d => d.GetAsync<CeaseDesistDraftDto>(
                    UserId,
                    draftId,
                    KeySpace,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(draft);

            var detailsDto = new TrademarkDetailsDto
            {
                Wordmark = "My Work",
                Owner = "TestOwner",
                SourceId = "testSource",
                RegistrationNumber = "R-123",
                Status = TrademarkStatusCategory.Registered,
                Provider = DataProvider.USPTO
            };
            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    PublicId,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(detailsDto);

            const string rawTemplate = "Hello {{SenderName}} about {{WorkTitle}}";

            letterTemplateProvider.Setup(
                t => t.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns(new LetterTemplatePreset(
                LetterTemplateType.CeaseAndDesist,
                KeySpace,
                DisplayName: "Trademark Cease & Desist",
                BodyTemplate: rawTemplate));

            templateReplacer.Setup(
                r => r.ReplaceTemplate(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>())).
                    Returns<string, IReadOnlyDictionary<string, string>>((
                        tpl,
                        placeholders) =>
                    {
                        var output = tpl;
                        foreach (var kv in placeholders)
                            output = output.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty);
                        return output;
                    });

            var result = await controller.CeaseDesistPreview(
                PublicId,
                draftId,
                CancellationToken.None);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().BeNull();

            var model =
                view.Model.Should().BeOfType<CeaseDesistViewModel>().Subject;

            model.PublicId.Should().Be(PublicId);
            model.BodyTemplate.Should().Contain("Alice");
            model.BodyTemplate.Should().Contain("My Work");

            draftStore.VerifyAll();
            trademarkSearchService.VerifyAll();
            letterTemplateProvider.VerifyAll();
            templateReplacer.VerifyAll();
        }
    }
}
