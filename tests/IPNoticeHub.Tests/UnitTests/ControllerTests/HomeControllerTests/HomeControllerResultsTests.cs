using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using IPNoticeHub.Web.Models.TrademarkSearch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.HomeControllerTests
{
    public class HomeControllerResultsTests : HomeControllerBase
    {
        [Test]
        public void HomeControllerIndex_ReturnsView()
        {
            var controller = TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.Index();

            actionResult.Should().BeOfType<ViewResult>();
            ((ViewResult)actionResult).Model.Should().BeNull();
        }

        [Test]
        public async Task SearchResults_MapsQueryAndReturnsViewModel()
        {
            var dto = new[]
            {
                new TrademarkSearchResultDto
                {
                    Id = 1,
                    PublicId = Guid.NewGuid(),
                    RegistrationNumber = "1234567",
                    Wordmark = "Anubis",
                    Owner = "Egypt Inc.",
                    Status = TrademarkStatusCategory.Registered.ToString()
                }
            };

            service.Setup(
                s => s.SearchAsync(
                It.IsAny<TrademarkSearchQueryDto>(),
                It.IsAny<CancellationToken>())).
                ReturnsAsync((dto, 1));

            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

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

            var viewModel = 
                (TrademarkSearchResultsViewModel) resultView.Model!;

            viewModel.Query.Should().Be("Anu");
            viewModel.Class.Should().Be((TrademarkClass?)25);
            viewModel.Status.Should().Be(TrademarkStatusCategory.Registered);
            viewModel.SearchBy.Should().Be(TrademarkSearchBy.Wordmark);
            viewModel.Office.Should().Be(DataProvider.USPTO);
            viewModel.Mode.Should().Be(SearchMode.Contains);
            viewModel.Total.Should().Be(1);
            viewModel.Results.Should().HaveCount(1);

            var itemViewModel = viewModel.Results.Single();

            itemViewModel.RegistrationNumber.Should().Be("1234567");
            itemViewModel.Wordmark.Should().Be("Anubis");
            itemViewModel.Owner.Should().Be("Egypt Inc.");
            itemViewModel.Status.Should().Be(TrademarkStatusCategory.Registered.ToString());
        }

        [Test]
        public async Task Results_WhenServiceReturnsEmpty_ShowsEmptyModelWithTotalZero()
        {
            service.Setup(
                s => s.SearchAsync(
                It.IsAny<TrademarkSearchQueryDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((new List<TrademarkSearchResultDto>(), 0));

            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

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

            var viewResult = actionResult as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<TrademarkSearchResultsViewModel>();

            var viewModel = 
                (TrademarkSearchResultsViewModel)viewResult.Model!;

            viewModel.Total.Should().Be(0);
            viewModel.Results.Should().BeEmpty();

            service.Verify(
                s => s.SearchAsync(
                It.IsAny<TrademarkSearchQueryDto>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task Results_UsesDefaultValues_WhenParametersAreMissing()
        {
            service.Setup(
                s => s.SearchAsync(
                It.IsAny<TrademarkSearchQueryDto>(), 
                It.IsAny<CancellationToken>())).
                ReturnsAsync((new List<TrademarkSearchResultDto>(), 0));

            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

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

            service.Verify(
                s => s.SearchAsync(
                It.Is<TrademarkSearchQueryDto>(q =>
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
