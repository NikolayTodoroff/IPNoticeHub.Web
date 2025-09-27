using FluentAssertions;
using IPNoticeHub.Services.Application.DTOs;
using IPNoticeHub.Tests.UnitTests.UnitTestUtilities;
using IPNoticeHub.Web.Controllers;
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
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            var items = new List<TrademarkWatchlistItemDTO>
            {
                new TrademarkWatchlistItemDTO
                {
                    Id = 101,
                    RegistrationNumber = "RN-101",
                    Wordmark = "WM1",
                    Owner = "O1",
                    CurrentStatus = "Registered"
                }
            };

            watchlistService.Setup(s => s.GetListByUserAsync("user-1", It.IsAny<CancellationToken>()))
               .ReturnsAsync(items);

            var actionResult = await controller.Index(CancellationToken.None);

            var resultView = actionResult as ViewResult;
            resultView.Should().NotBeNull();
            resultView!.Model.Should().NotBeNull();
            resultView.Model.Should().BeOfType<WatchlistIndexViewModel>();

            var indexViewModel = (WatchlistIndexViewModel)resultView.Model!;
            indexViewModel.Items.Should().HaveCount(1);
            indexViewModel.Items[0].Id.Should().Be(101);

            watchlistService.Verify(s => s.GetListByUserAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Index_WithoutUser_ReturnsUnauthorized()
        {
            var (controller, watchlistService, httpContext) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: false);

            var actionResult = await controller.Index(CancellationToken.None);

            actionResult.Should().BeOfType<UnauthorizedResult>();
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_OnSuccess_SetsSuccessTempData_AndRedirectsToIndex()
        {
            var (controller, watchlistService, _) = TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

            watchlistService.Setup(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.Add(trademarkId, returnUrl: null, CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>()
                  .Which.ActionName.Should().Be(nameof(WatchlistController.Index));

            controller.TempData["Success"].Should().NotBeNull();
            controller.TempData["Success"]!.ToString()!.Should().Contain("Added to Watchlist");

            watchlistService.Verify(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.Verify(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WhenAlreadyExists_SetsInfoTempData_AndDoesNotCallAdd_RedirectsToIndex()
        {
            var (controller, watchlistService, _) = TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 456;

            watchlistService.Setup(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            var actionResult = await controller.Add(trademarkId, returnUrl: null, CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>()
                  .Which.ActionName.Should().Be(nameof(WatchlistController.Index));

            controller.TempData["Info"].Should().NotBeNull();

            watchlistService.Verify(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.Verify(s => s.AddAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Remove_SetsSuccessTempData_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 222;

            watchlistService.Setup(s => s.RemoveAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.Remove(trademarkId, CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["Success"]!.ToString().Should().Contain("Removed");

            watchlistService.Verify(s => s.RemoveAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_Enable_SetsEnabledMessage_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 333;

            watchlistService.Setup(s => s.ToggleNotificationsAsync("user-1", trademarkId, true, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.ToggleNotifications(trademarkId, notificationsEnabled: true, CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["Success"]!.ToString().Should().Contain("enabled");

            watchlistService.Verify(s => s.ToggleNotificationsAsync("user-1", trademarkId, true, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_Disable_SetsDisabledMessage_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 444;

            watchlistService.Setup(s => s.ToggleNotificationsAsync("user-1", trademarkId, false, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.ToggleNotifications(trademarkId, notificationsEnabled: false, CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["Success"]!.ToString().Should().Contain("disabled");

            watchlistService.Verify(s => s.ToggleNotificationsAsync("user-1", trademarkId, false, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotifications_OnFailure_SetsError_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 555;

            watchlistService.Setup(s => s.ToggleNotificationsAsync("user-1", trademarkId, true, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("boom"));

            var result = await controller.ToggleNotifications(trademarkId, notificationsEnabled: true, CancellationToken.None);

            result.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["Error"]!.ToString().Should().Contain("Failed");

            watchlistService.Verify(s => s.ToggleNotificationsAsync("user-1", trademarkId, true, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_OnFailure_SetsErrorTempData_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            watchlistService.Setup(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("boom"));

            var actionResult = await controller.Add(trademarkId, returnUrl: null, CancellationToken.None);

            var redirectResult = actionResult.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be(nameof(controller.Index));

            controller.TempData["Error"].Should().NotBeNull();
            controller.TempData["Error"]!.ToString()!.Should().Contain("Could not add to Watchlist.");

            watchlistService.Verify(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.Verify(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_WhenDuplicate_SetsInfoTempData_AndRedirectsBackToReturnUrl()
        {
            const int trademarkId = 456;
            const string returnUrl = "/Trademarks/Details/00000000-0000-0000-0000-000000000001";

            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);
            controller.Url = url.Object;

            watchlistService.Setup(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var actionResult = await controller.Add(trademarkId, returnUrl, CancellationToken.None);

            var redirectResult = actionResult.Should().BeOfType<RedirectResult>().Subject;
            redirectResult.Url.Should().Be(returnUrl);

            controller.TempData["Info"].Should().NotBeNull();
            controller.TempData["Info"]!.ToString()!.Should().Contain("Trademark is already in your watchlist.");

            controller.TempData.ContainsKey("Success").Should().BeFalse();

            watchlistService.Verify(s => s.ExistsAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.Verify(s => s.AddAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            watchlistService.VerifyNoOtherCalls();
        }
    }
}
