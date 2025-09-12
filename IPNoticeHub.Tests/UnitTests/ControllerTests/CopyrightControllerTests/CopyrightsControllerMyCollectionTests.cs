using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Copyrights.Abstractions;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.CopyrightControllerTests
{
    /// <summary>
    /// Section: CopyrightsController – MyCollection
    ///  - When user present: calls service, sets ViewBag.SortBy, returns view with page.
    ///  - When no user: returns Forbid.
    /// </summary>
    [TestFixture]
    public class CopyrightsControllerMyCollectionTests
    {
        [Test]
        public async Task MyCollection_WithUser_ReturnsViewAndSetsSortCorrectly()
        {
            const string userId = "u1";
            const CollectionSortBy sortBy = CollectionSortBy.TitleAsc;

            var pagedResult = new PagedResult<CopyrightListItemDTO>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<CopyrightListItemDTO>()
            };

            var copyrightService = new Mock<ICopyrightService>();

            copyrightService.Setup(s => s.GetUserCollectionAsync(userId, sortBy, 1, 10, It.IsAny<CancellationToken>()))
               .ReturnsAsync(pagedResult);

            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId);

            var myCollectionActionResult = await controller.MyCollection(sortBy, 1, 10);

            var viewResult = myCollectionActionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.Model.Should().Be(pagedResult);
            ((CollectionSortBy)controller.ViewBag.SortBy).Should().Be(sortBy);

            copyrightService.Verify(s => s.GetUserCollectionAsync(userId, sortBy, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MyCollection_NoUser_ReturnsForbid()
        {
            var copyrightService = new Mock<ICopyrightService>(MockBehavior.Strict);
            var controller = TestCopyrightControllerFactory.CreateController(copyrightService.Object, userId: null);

            var myCollectionActionResult = await controller.MyCollection();

            myCollectionActionResult.Should().BeOfType<ForbidResult>();
            copyrightService.VerifyNoOtherCalls();
        }
    }
}
