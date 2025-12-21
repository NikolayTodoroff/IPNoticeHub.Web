using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Web.Models.Copyrights;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.CopyrightDTOs;
using IPNoticeHub.Application.Services.CopyrightServices.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    [TestFixture]
    public class MyCollectionCopyrightsControllerTests
    {
        [Test]
        public async Task MyCollection_WithUser_ReturnsViewAndSetsSortCorrectly()
        {
            const string userId = "u1";
            const CollectionSortBy sortBy = CollectionSortBy.TitleAsc;

            var expectedPagedResult = 
                new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var service = 
                new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(expectedPagedResult);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId);

            var actionResult = 
                await controller.MyCollection(
                sortBy, 
                1, 
                10);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().BeOfType<CopyrightCollectionViewModel>();

            ((CollectionSortBy)controller.ViewBag.SortBy).Should().Be(sortBy);

            service.Verify(
                s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task MyCollection_NoUser_ReturnsForbid()
        {
            var service = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: null);

            var actionResult = await controller.MyCollection();
            actionResult.Should().BeOfType<ForbidResult>();
            service.VerifyNoOtherCalls();
        }

        [Test]
        public async Task MyCollection_UsesDefaultPaging_WhenNoArgumentsProvided()
        {
            var expectedPagedResult = 
                new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var service = new Mock<ICopyrightService>();

            service.Setup(
                s => s.GetUserCollectionAsync(
                "u1",
                CollectionSortBy.DateAddedDesc,
                DefaultPage,
                DefaultPageSize,
                It.IsAny<CancellationToken>())).
                ReturnsAsync(expectedPagedResult);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                service.Object, 
                userId: "u1");

            var actionResult =  await controller.MyCollection();

            actionResult.Should().BeOfType<ViewResult>();

            service.Verify(
                s => s.GetUserCollectionAsync(
                "u1",
                CollectionSortBy.DateAddedDesc,
                DefaultPage,
                DefaultPageSize,
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
