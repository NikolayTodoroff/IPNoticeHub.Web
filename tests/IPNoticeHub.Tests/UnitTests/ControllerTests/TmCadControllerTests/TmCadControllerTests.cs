using FluentAssertions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
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
    public class TmCadControllerTests : TmCadControllerBase
    {
        [Test]
        public async Task CeaseDesist_Get_ShouldReturnViewModel_WhenTrademarkExistsAndInCollection()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456",
            };

            letterTemplateProvider.
                Setup(p => p.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns(new LetterTemplatePreset(
                    LetterTemplateType.CeaseAndDesist,
                    TrademarkCeaseDesistKey,
                    "Trademark Cease & Desist",
                    "Test Template Body"
                    ));

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(trademarkDto);

            trademarkCollectionService.Setup(
                s => s.IsInCollectionAsync(
                    UserId, 
                    trademarkDto.Id, 
                    false, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(true);

            var result = await controller.CeaseDesist(publicId);

            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<CeaseDesistViewModel>();

            var model = viewResult.Model as CeaseDesistViewModel;

            model!.PublicId.Should().Be(publicId);
            model.WorkTitle.Should().Be("Test Trademark");
            model.RegistrationNumber.Should().Be("REG123456");
            model.BodyTemplate.Should().Be("Test Template Body");
            controller.ViewData["ShowAdditionalFacts"].Should().Be(true);
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnNotFound_WhenTrademarkDoesNotExist()
        {
            var publicId = Guid.NewGuid();

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((TrademarkDetailsDto?)null);

            var result = await controller.CeaseDesist(publicId);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnNotFound_WhenTrademarkNotInCollection()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(trademarkDto);

            trademarkCollectionService.Setup(
                s => s.IsInCollectionAsync(
                    UserId, 
                    trademarkDto.Id,
                    false, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(false);

            var result = await controller.CeaseDesist(publicId);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldReturnForbid_WhenUserIdNotFound()
        {
            var publicId = Guid.NewGuid();

            controller.ControllerContext.HttpContext.User = 
                new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.CeaseDesist(publicId);
            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task CeaseDesist_Get_ShouldUseEmptyTemplate_WhenTemplateNotFound()
        {
            var publicId = Guid.NewGuid();
            var trademarkDto = new TrademarkDetailsDto
            {
                Id = 1,
                Wordmark = "Test Trademark",
                RegistrationNumber = "REG123456"
            };

            trademarkSearchService.Setup(
                s => s.GetDetailsAsync(
                    publicId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(trademarkDto);

            trademarkCollectionService.Setup(
                s => s.IsInCollectionAsync(
                    UserId,
                    trademarkDto.Id,
                    false,
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(true);

            letterTemplateProvider.Setup(
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns((LetterTemplatePreset?)null);

            var result = await controller.CeaseDesist(publicId);

            var viewResult = result as ViewResult;
            var model = viewResult!.Model as CeaseDesistViewModel;
            model!.BodyTemplate.Should().BeEmpty();
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnFileResult_WhenModelStateIsValid()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                SenderName = "John Doe",
                RecipientName = "Jane Smith"
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

            pdfService.Setup(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(pdfBytes);

            var result = await controller.CeaseDesist(publicId, viewModel);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;

            fileResult!.ContentType.Should().Be("application/pdf");
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);
            fileResult.FileDownloadName.Should().Contain("CeaseDesist-Test Work");
            fileResult.FileDownloadName.Should().Contain(".pdf");
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId
            };

            controller.ModelState.AddModelError("SenderName", "Required");
            var result = await controller.CeaseDesist(publicId, viewModel);

            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().Be(viewModel);
        }

        [Test]
        public async Task CeaseDesist_Post_ShouldCallPdfServiceWithCorrectInput()
        {
            var publicId = Guid.NewGuid();
            var viewModel = new CeaseDesistViewModel
            {
                PublicId = publicId,
                WorkTitle = "Test Work",
                SenderName = "John Doe",
                RecipientName = "Jane Smith"
            };

            var pdfBytes = new byte[] { 1, 2, 3 };

            pdfService.Setup(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(pdfBytes);

            await controller.CeaseDesist(publicId, viewModel);

            pdfService.Verify(
                s => s.GenerateFromInputAsync(
                    It.IsAny<LetterInputDto>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}