using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Common.Infrastructure.Paging;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Web.Models.Copyrights;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    /// <summary>
    /// Section: CopyrightsController – Index
    ///  - When user present: calls service, sets ViewBag.SortBy, returns view with page.
    ///  - When no user: returns Forbid.
    /// </summary>
    [TestFixture]
    public class CopyrightsControllerIndexTests
    {
        [Test]
        public async Task Index_WithUser_ReturnsViewAndSetsSortCorrectly()
        {
            const string userId = "u1";
            const CollectionSortBy sortBy = CollectionSortBy.TitleAsc;

            var dto = new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, 
                It.IsAny<CancellationToken>())).
            ReturnsAsync(dto);

            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId);

            var indexActionResult = await controller.MyCollection(
                sortBy, 
                1, 
                10);

            var viewResult = indexActionResult.Should().
                BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().
                BeOfType<CopyrightCollectionViewModel>();

            ((CollectionSortBy)controller.ViewBag.SortBy).Should().
                Be(sortBy);

            copyrightService.Verify(s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Index_NoUser_ReturnsForbid()
        {
            var copyrightService = 
                new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: null);

            var indexActionResult = await controller.MyCollection();

            indexActionResult.Should().
                BeOfType<ForbidResult>();

            copyrightService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_UsesDefaultPaging_WhenNoArgumentsProvided()
        {
            var pagedResult = new PagedResult<CopyrightSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightSingleItemDto>()
            };

            var copyrightService = new Mock<ICopyrightService>();
            copyrightService.Setup(s => s.GetUserCollectionAsync(
                "u1",
                CollectionSortBy.DateAddedDesc,
                DefaultPage,
                DefaultPageSize,
                It.IsAny<CancellationToken>())).
            ReturnsAsync(pagedResult);

            var controller = TestCopyrightControllerFactory.CreateController(
                copyrightService.Object, 
                userId: "u1");

            var indexActionResult = await controller.MyCollection();

            indexActionResult.Should().
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
