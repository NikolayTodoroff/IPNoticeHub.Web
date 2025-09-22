using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Web.Models.Application;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.HomeControllerTests
{
    /// <summary>
    /// Section: HomeController – Trademark Search Results
    ///  - Verifies that the Results action maps the query parameters correctly, calls the service, and returns a populated view model with the expected data.
    ///  - Verifies that the Results action returns an empty view model with zero total when the service returns no results.
    ///  - Verifies that the Results action in the HomeController uses default values for query parameters when they are not provided.
    /// </summary>
    [TestFixture]
    public class HomeControllerResultsTests
    {
        [Test]
        public async Task SearchResults_MapsQueryAndReturnsViewModel()
        {
            var tmSearchService = new Mock<ITrademarkSearchQueryService>(MockBehavior.Strict);

            var searchResultsDTO = new[]
            {
                new TrademarkSearchResultDTO
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = "1234567",
                    Wordmark = "Anubis",
                    Owner = "Egypt Inc.",
                    Status = TrademarkStatusCategory.Registered.ToString()
                }
            };

            tmSearchService.Setup(s => s.SearchAsync(It.IsAny<TrademarkSearchQueryDTO>(),It.IsAny<CancellationToken>())).
                ReturnsAsync((searchResultsDTO, 1));

            var controller = TestHomeControllerFactory.CreateHomeController(tmSearchService.Object);

            var actionResult = await controller.Results(
                trademark: "Anu",
                classNumber: (TrademarkClass?)25,
                status: TrademarkStatusCategory.Registered,
                searchByItem: TrademarkSearchBy.Wordmark,
                office: DataProvider.USPTO,
                mode: SearchMode.Contains,
                currentPage: 2,
                pageSize: 10,
                cancellationToken: CancellationToken.None);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();

            resultView!.Model.Should().BeOfType<TrademarkSearchResultsViewModel>();
            var resultViewModel = (TrademarkSearchResultsViewModel)resultView.Model!;

            resultViewModel.Query.Should().Be("Anu");
            resultViewModel.Class.Should().Be((TrademarkClass?)25);
            resultViewModel.Status.Should().Be(TrademarkStatusCategory.Registered);
            resultViewModel.SearchBy.Should().Be(TrademarkSearchBy.Wordmark);
            resultViewModel.Office.Should().Be(DataProvider.USPTO);
            resultViewModel.Mode.Should().Be(SearchMode.Contains);
            resultViewModel.Total.Should().Be(1);

            resultViewModel.Results.Should().HaveCount(1);
            var item = resultViewModel.Results.Single();
            item.RegistrationNumber.Should().Be("1234567");
            item.Wordmark.Should().Be("Anubis");
            item.Owner.Should().Be("Egypt Inc.");
            item.Status.Should().Be(TrademarkStatusCategory.Registered.ToString());
        }

        [Test]
        public async Task Results_WhenServiceReturnsEmpty_ShowsEmptyModelWithTotalZero()
        {
            var tmSearchService = new Mock<ITrademarkSearchQueryService>(MockBehavior.Strict);

            tmSearchService.Setup(s => s.SearchAsync(It.IsAny<TrademarkSearchQueryDTO>(), It.IsAny<CancellationToken>())).
                ReturnsAsync((new List<TrademarkSearchResultDTO>(), 0));

            var controller = TestHomeControllerFactory.CreateHomeController(tmSearchService.Object);

            var actionResult = await controller.Results(
                trademark: "",
                classNumber: null,
                status: null,
                searchByItem: TrademarkSearchBy.Wordmark,
                office: null,
                mode: SearchMode.Contains,
                currentPage: 1,
                pageSize: 25,
                cancellationToken: CancellationToken.None);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();

            resultView!.Model.Should().BeOfType<TrademarkSearchResultsViewModel>();
            var resultViewModel = (TrademarkSearchResultsViewModel)resultView.Model!;

            resultViewModel.Total.Should().Be(0);
            resultViewModel.Results.Should().BeEmpty();

            tmSearchService.Verify(s => s.SearchAsync(It.IsAny<TrademarkSearchQueryDTO>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Results_UsesDefaultValues_WhenParametersAreMissing()
        {
            var tmSearchService = new Mock<ITrademarkSearchQueryService>(MockBehavior.Strict);

            tmSearchService.Setup(s => s.SearchAsync(It.IsAny<TrademarkSearchQueryDTO>(), It.IsAny<CancellationToken>())).
                ReturnsAsync((new List<TrademarkSearchResultDTO>(), 0));

            var controller = TestHomeControllerFactory.CreateHomeController(tmSearchService.Object);

            var actionResult = await controller.Results(
                trademark: null,
                classNumber: null,
                status: null,
                searchByItem: null,
                office: null,
                mode: null,
                cancellationToken: CancellationToken.None);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();
            resultView!.Model.Should().BeOfType<TrademarkSearchResultsViewModel>();

            tmSearchService.Verify(s => s.SearchAsync(
                It.Is<TrademarkSearchQueryDTO>(q =>
                    q.Query == null &&
                    q.Class == null &&
                    q.Status == null &&
                    q.SearchBy == null &&
                    q.Office == null &&
                    q.Mode == null &&
                    q.Page == DefaultPage &&
                    q.PageSize == DefaultPageSize),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
