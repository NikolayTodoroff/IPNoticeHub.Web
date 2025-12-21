using FluentAssertions;
using IPNoticeHub.Application.DTOs.WatchlistDTOs;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.Watchlist;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.WatchlistControllerTests
{
    [TestFixture]
    public class WatchlistControllerTests
    {
        [Test]
        public async Task Index_WhenUserIsPresent_ReturnsViewWithModel()
        {
            var (controller,
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            var items = new List<WatchlistItemDto>
            {
                new WatchlistItemDto
                {
                    Id = 101,
                    RegistrationNumber = "RN-101",
                    Wordmark = "WM1",
                    Owner = "O1",
                    CurrentStatus = "Registered"
                }
            };

            watchlistService.Setup(s => s.GetListByUserAsync(
                "user-1", 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(items);

            var actionResult = 
                await controller.Index(CancellationToken.None);

            var resultView = actionResult as ViewResult;

            resultView.Should().
                NotBeNull();

            resultView!.Model.Should().
                NotBeNull();

            resultView.Model.Should().
                BeOfType<WatchlistIndexViewModel>();

            var indexViewModel = 
                (WatchlistIndexViewModel)resultView.Model!;

            indexViewModel.Items.Should().
                HaveCount(1);

            indexViewModel.Items[0].Id.Should().
                Be(101);

            watchlistService.Verify(s => s.GetListByUserAsync(
                "user-1", 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_WithoutUser_ReturnsUnauthorized()
        {
            var (controller, 
                watchlistService, 
                httpContext) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: false);

            var actionResult = await controller.Index(CancellationToken.None);

            actionResult.Should().
                BeOfType<UnauthorizedResult>();

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_OnSuccess_SetsSuccessTempData_AndRedirectsToIndex()
        {
            var (controller, watchlistService, _) = 
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            watchlistService.Setup(s => s.AddAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var actionResult = await controller.Add(
                trademarkId, 
                returnUrl: null, 
                CancellationToken.None);

            actionResult.Should().
                BeOfType<RedirectToActionResult>().
                  Which.ActionName.Should().
                  Be(nameof(WatchlistController.Index));

            controller.TempData["SuccessMessage"].Should().
                NotBeNull();

            controller.TempData["SuccessMessage"]!.ToString()!.Should().
                Contain("added to your watchlist");

            watchlistService.Verify(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.Verify(s => s.AddAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WhenAlreadyExists_SetsInfoTempData_AndDoesNotCallAdd_RedirectsToIndex()
        {
            var (controller,
                watchlistService, _) = 
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 456;

            watchlistService.Setup(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var actionResult = await controller.Add(
                trademarkId, 
                returnUrl: null, 
                CancellationToken.None);

            actionResult.Should().
                BeOfType<RedirectToActionResult>().
                Which.ActionName.Should().
                Be(nameof(WatchlistController.Index));

            controller.TempData["InfoMessage"].Should().
                NotBeNull();

            watchlistService.Verify(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.Verify(s => s.AddAsync(
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Remove_SetsSuccessTempData_AndRedirects()
        {
            var (controller, 
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 222;
            var returnUrl = "/Watchlist/Index?page=4";

            watchlistService.Setup(s => s.RemoveAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var actionResult = await controller.Remove(
                trademarkId, 
                returnUrl,
                CancellationToken.None);

            actionResult.Should().
                BeOfType<LocalRedirectResult>().
                Which.Url.Should().
                Be(returnUrl);

            controller.TempData["SuccessMessage"]!.ToString().Should().
                Contain("removed from your watchlist");

            watchlistService.Verify(s => s.RemoveAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_Enable_SetsEnabledMessage_AndRedirects()
        {
            var (controller, 
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 333;
            var returnUrl = "/Watchlist/Index?page=3";

            watchlistService.Setup(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                true, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var actionResult = 
                await controller.ToggleNotifications(
                trademarkId, 
                true, returnUrl, 
                CancellationToken.None);

            actionResult.Should().
                BeOfType<LocalRedirectResult>().
                Which.Url.Should().Be(returnUrl);

            controller.TempData["SuccessMessage"]!.ToString().Should().
                Contain("enabled");

            watchlistService.Verify(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                true, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_Disable_SetsDisabledMessage_AndRedirects()
        {
            var (controller, 
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 444;
            var returnUrl = "/Watchlist/Index?page=1";

            watchlistService.Setup(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                false, 
                It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var actionResult = await controller.ToggleNotifications(
                trademarkId, 
                false, 
                returnUrl,
                CancellationToken.None);

            actionResult.Should().
                BeOfType<LocalRedirectResult>().Which.Url.Should().
                Be(returnUrl);

            controller.TempData["SuccessMessage"]!.ToString().Should().
                Contain("disabled");

            watchlistService.Verify(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                false, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_OnFailure_SetsError_AndRedirects()
        {
            var (controller, 
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 555;
            var returnUrl = "/Watchlist/Index?page=2";

            watchlistService.Setup(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                true, 
                It.IsAny<CancellationToken>())).
                ThrowsAsync(new Exception("boom"));

            var result = 
                await controller.ToggleNotifications(
                trademarkId, 
                enabled: true, 
                returnUrl: returnUrl, 
                CancellationToken.None);

            result.Should().BeOfType<LocalRedirectResult>().
                Which.Url.Should().
                Be(returnUrl);

            controller.TempData["ErrorMessage"]!.ToString().Should().
                Contain("Failed");

            watchlistService.Verify(s => s.ToggleNotificationsAsync(
                "user-1", 
                trademarkId, 
                true, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_OnFailure_SetsErrorTempData_AndRedirects()
        {
            var (controller, 
                watchlistService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            watchlistService.Setup(s => s.AddAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                ThrowsAsync(new InvalidOperationException("boom"));

            var actionResult = await controller.Add(
                trademarkId, 
                returnUrl: null, 
                CancellationToken.None);

            var redirectResult = actionResult.Should().
                BeOfType<RedirectToActionResult>().Subject;

            redirectResult.ActionName.Should().
                Be(nameof(controller.Index));

            controller.TempData["ErrorMessage"].Should().
                NotBeNull();

            controller.TempData["ErrorMessage"]!.ToString()!.Should().
                Contain("Could not add to Watchlist.");

            watchlistService.Verify(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.Verify(s => s.AddAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WhenDuplicate_SetsInfoTempData_AndRedirectsBackToReturnUrl()
        {
            const int trademarkId = 456;
            const string returnUrl = 
                "/Trademarks/Details/00000000-0000-0000-0000-000000000001";

            var (controller, 
                watchListService, _) =
                TestWatchlistControllerFactory.
                CreateWatchlistController(userExists: true);

            controller.ConfigureUrlHelper(
                returnUrl: returnUrl, 
                isLocal: true);

            watchListService.Setup(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>())).
                ReturnsAsync(true);

            var actionResult = await controller.Add(
                trademarkId, 
                returnUrl, 
                CancellationToken.None);

            var redirect = actionResult.Should().
                BeOfType<LocalRedirectResult>().Subject;

            redirect.Url.Should().
                Be(returnUrl);

            controller.TempData["InfoMessage"].Should().
                NotBeNull();

            controller.TempData["InfoMessage"]!.ToString().Should().
                Contain("already in your watchlist");

            watchListService.Verify(s => s.ExistsAsync(
                "user-1", 
                trademarkId, 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            watchListService.VerifyNoOtherCalls();
        }
    }
}
