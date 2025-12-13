using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
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
    public class CopyrightsControllerMyCollectionTests
    {
        [Test]
        public async Task MyCollection_WithUser_ReturnsViewAndSetsSortCorrectly()
        {
            const string userId = "u1";
            const CollectionSortBy sortBy = CollectionSortBy.TitleAsc;

            var pagedResult = 
                new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(
                s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(pagedResult);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId);

            var myCollectionActionResult = 
                await controller.MyCollection(
                sortBy, 
                1, 
                10);

            var viewResult = myCollectionActionResult.Should().
                BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().
                BeOfType<CopyrightCollectionViewModel>();

            ((CollectionSortBy)controller.ViewBag.SortBy).Should().
                Be(sortBy);

            copyrightService.Verify(
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
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: null);

            var myCollectionActionResult = 
                await controller.MyCollection();

            myCollectionActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task MyCollection_UsesDefaultPaging_WhenNoArgumentsProvided()
        {
            var dto = 
                new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var copyrightService = 
                new Mock<ICopyrightService>();

            copyrightService.Setup(
                s => s.GetUserCollectionAsync(
                "u1",
                CollectionSortBy.DateAddedDesc,
                DefaultPage,
                DefaultPageSize,
                It.IsAny<CancellationToken>())).
            ReturnsAsync(dto);

            var controller = 
                TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var myCollectionActionResult = 
                await controller.MyCollection();

            myCollectionActionResult.Should().
                BeOfType<ViewResult>();

            copyrightService.Verify(
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
