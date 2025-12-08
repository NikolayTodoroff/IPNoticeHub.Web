using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;
using IPNoticeHub.Services.Trademarks.Abstractions;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Models.Trademarks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.TrademarkControllerTests
{
    [TestFixture]
    public class TmControllerMyCollectionTests
    {
        [Test]
        public async Task MyCollection_WhenUserMissing_ReturnsForbid()
        {
            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>(MockBehavior.Strict);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                userId: null);

            var myCollectionActionResult = await controller.MyCollection();

            myCollectionActionResult.Should().
                BeOfType<ForbidResult>();

            tmCollectionService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task MyCollection_WhenUserPresent_ReturnsViewWithPage_AndSetsSortBag()
        {
            var pagedResult = new PagedResult<TrademarkSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<TrademarkSingleItemDto>()
            };       

            const string userId = "u1";
            const int currentPage = 1;
            const int resultsPerPage = 10;
            const CollectionSortBy sortBy = CollectionSortBy.DateAddedDesc;

            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                currentPage, 
                resultsPerPage, It.IsAny<CancellationToken>())).
                ReturnsAsync(pagedResult);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                userId: "u1");

            var result = await controller.MyCollection(
                sortBy, 
                currentPage, 
                resultsPerPage);

            var myCollectionView = result.Should().
                BeOfType<ViewResult>().Subject;

            myCollectionView.Model.Should().
                BeOfType<TrademarkCollectionViewModel>();


            var sortInBag = (CollectionSortBy)controller.ViewBag.SortBy;

            sortInBag.Should().
                Be(sortBy);

            tmCollectionService.Verify(s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                currentPage, 
                resultsPerPage, 
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task MyCollection_WithNonDefaultSort_SetsThatSortInViewBag()
        {
            var pagedResult = new PagedResult<TrademarkSingleItemDto>
            {
                ResultsCount = 0,
                CurrentPage = 1,
                ResultsCountPerPage = 10,
                Results = new List<TrademarkSingleItemDto>()
            };

            const string userId = "u1";
            const CollectionSortBy sortBy = CollectionSortBy.WordmarkAsc;

            var tmCollectionService = 
                new Mock<ITrademarkCollectionService>();

            tmCollectionService.Setup(s => s.GetUserCollectionAsync(
                userId, 
                sortBy, 
                1, 
                10, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(pagedResult);

            var controller = TestTrademarkControllerFactory.CreateTrademarksController(
                tmCollectionService.Object, 
                userId: userId);

            var myCollectionActionResult = await controller.MyCollection(
                sortBy, 
                currentPage: 1, 
                resultsPerPage: 10);

            var myCollectionView = myCollectionActionResult as ViewResult;

            myCollectionView.Should().
                NotBeNull();

            myCollectionView.Model.Should().
                BeOfType<TrademarkCollectionViewModel>();

            var sortInBag = (CollectionSortBy)controller.ViewBag.SortBy;

            sortInBag.Should().
                Be(sortBy);
        }
    }
}
