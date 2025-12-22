using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.DTOs.DraftStoreDTOs;
using IPNoticeHub.Application.Rendering.Abstractions;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Abstractions;
using IPNoticeHub.Application.Services.DocumentLibraryService.Implementations;
using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Shared.Constants;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.PdfGeneration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightCadControllerTests
{
    public class PreviewCopyrightCadControllerTests : BaseCopyrightCadControllerTests
    {
        public class CopyrightCadController_GetPreview_Tests
        {
            private const string UserId = "user-123";
            private static readonly Guid PublicId = Guid.NewGuid();

            private const string KeySpace = InputDraftConstants.UserInputDraftOptions.CopyrightCadKeySpace;
            private const string TemplateKey = CopyrightCeaseDesistKey;

            private Mock<ICopyrightService> copyrightService = null!;
            private Mock<IPdfLetterService> pdfService = null!;
            private Mock<ILetterTemplateProvider> letterTemplateProvider = null!;
            private Mock<IDocumentLibraryService> documentLibraryService = null!;
            private Mock<ITemplateTokenReplacer> templateReplacer = null!;
            private Mock<IUserInputDraftStore> draftStore = null!;

            private CopyrightCadController controller = null!;

            [SetUp]
            public void SetUp()
            {
                copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
                pdfService = new Mock<IPdfLetterService>(MockBehavior.Strict);
                letterTemplateProvider = new Mock<ILetterTemplateProvider>(MockBehavior.Strict);
                documentLibraryService = new Mock<IDocumentLibraryService>(MockBehavior.Strict);
                templateReplacer = new Mock<ITemplateTokenReplacer>(MockBehavior.Strict);
                draftStore = new Mock<IUserInputDraftStore>(MockBehavior.Strict);

                controller = new CopyrightCadController(
                    copyrightService: copyrightService.Object,
                    pdfService: pdfService.Object,
                    letterTemplateProvider: letterTemplateProvider.Object,
                    documentLibraryService: documentLibraryService.Object,
                    templateReplacer: templateReplacer.Object,
                    draftStore: draftStore.Object
                );

                var http = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "test"))
                };

                controller.ControllerContext = new ControllerContext { HttpContext = http };
                controller.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());
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

                var actionResult = await controller.CeaseDesistPreview(
                    viewModel,
                    CancellationToken.None);

                actionResult.Should().BeOfType<ViewResult>();
                var viewResult = (ViewResult)actionResult;

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

                var viewModel = new CeaseDesistViewModel
                {
                    PublicId = Guid.NewGuid(),
                    BodyTemplate = "Final body"
                };

                var result = await controller.CeaseDesistPreview(viewModel);

                result.Should().BeOfType<ForbidResult>();

                copyrightService.Verify(
                    s => s.GetDetailsAsync(
                        It.IsAny<string>(),
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
                    Times.Never);
            }

            [Test]
            public async Task CeaseDesistPreview_Post_ShouldSaveDraft_AndRedirectToPreview_WhenValid()
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
                
                copyrightService.Verify(
                    s => s.GetDetailsAsync(
                        It.IsAny<string>(), 
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
            public async Task CeaseDesistPreview_Post_ShouldClearBodyTemplate_BeforeSaving()
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
            public async Task GetPreview_ShouldReturnForbid_WhenUserMissing()
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
            public async Task GetPreview_ShouldRedirectToCeaseDesist_WhenDraftIdMissing()
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
                copyrightService.VerifyNoOtherCalls();
            }

            [Test]
            public async Task GetPreview_ShouldRedirect_WhenDraftMissingOrExpired()
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
                copyrightService.VerifyNoOtherCalls();
            }

            [Test]
            public async Task GetPreview_ShouldRedirect_WhenDetailsMissing()
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

                copyrightService.Setup(
                    s => s.GetDetailsAsync(
                        UserId, 
                        PublicId, 
                        It.IsAny<CancellationToken>())).
                        ReturnsAsync((CopyrightDetailsDto)null!);

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
                copyrightService.VerifyAll();
                letterTemplateProvider.VerifyNoOtherCalls();
                templateReplacer.VerifyNoOtherCalls();
            }

            [Test]
            public async Task GetPreview_ShouldReturnView_WithComposedBody_WhenHappyPath()
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

                var detailsDto = new CopyrightDetailsDto
                {
                    Title = "My Work",
                    RegistrationNumber = "R-123",
                    TypeOfWork = "Software",
                    YearOfCreation = 2010,
                    DateOfPublication = new DateTime(2020,01,01),
                    Owner = "TestOwner",
                    NationOfFirstPublication = "USA"
                };
                copyrightService.Setup(
                    s => s.GetDetailsAsync(
                        UserId, 
                        PublicId, 
                        It.IsAny<CancellationToken>())).
                        ReturnsAsync(detailsDto);

                const string rawTemplate = "Hello {{SenderName}} about {{WorkTitle}}";

                letterTemplateProvider.Setup(
                    t => t.GetTemplateByKey(TemplateKey)).
                    Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    CopyrightCeaseDesistKey,
                    DisplayName:"Copyright Cease & Desist",
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
                copyrightService.VerifyAll();
                letterTemplateProvider.VerifyAll();
                templateReplacer.VerifyAll();
            }
        }
    }
}
