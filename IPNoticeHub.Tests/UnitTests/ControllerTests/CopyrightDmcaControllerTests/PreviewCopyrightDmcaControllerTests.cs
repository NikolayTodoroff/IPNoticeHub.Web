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

        
    }
}
