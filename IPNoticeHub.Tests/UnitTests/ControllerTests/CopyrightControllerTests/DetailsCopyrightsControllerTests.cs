using FluentAssertions;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using IPNoticeHub.Web.Models.Copyrights;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class DetailsCopyrightsControllerTests
    {
        [Test]
        public async Task Details_Success_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            var dto = new CopyrightDetailsDto
            {
                PublicId = id,
                RegistrationNumber = "TX-9",
                TypeOfWork = "Literary",
                Title = "testTitle",
                Owner = "testOwner"
            };

            var service = new Mock<ICopyrightService>();

            service.Setup(s => s.GetDetailsAsync(
                "u1", 
                id, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(dto);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult = await controller.Details(id);

            var viewModel = 
                actionResult.Should(). BeOfType<ViewResult>().Subject;

            viewModel.Model.Should().
                BeEquivalentTo(new CopyrightDetailsViewModel
            {
                PublicId = dto.PublicId,
                RegistrationNumber = dto.RegistrationNumber,
                TypeOfWork = dto.TypeOfWork,
                Title = dto.Title,
                Owner = dto.Owner
            });
        }

        [Test]
        public async Task Details_NoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: null);

            var actionResult = 
                await controller.Details(Guid.NewGuid());

            actionResult.Should().BeOfType<ForbidResult>();
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            var service = new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetDetailsAsync(
                "u1", 
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((CopyrightDetailsDto?)null);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var detailsActionResult = 
                await controller.Details(Guid.NewGuid());

            detailsActionResult.Should().BeOfType<NotFoundResult>();
        }
    }
}
