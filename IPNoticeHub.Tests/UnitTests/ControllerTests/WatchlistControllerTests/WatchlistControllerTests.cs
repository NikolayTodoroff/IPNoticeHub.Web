using FluentAssertions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.DTOs;
using IPNoticeHub.Tests.UnitTests.TestUtilities;
using IPNoticeHub.Tests.UnitTests.UnitTestUtilities;
using IPNoticeHub.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.AddAsync("user-1", trademarkId,It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.Add(trademarkId, null,CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = (RedirectToActionResult) actionResult;
            redirectToActionResult.ActionName.Should().Be(nameof(WatchlistController.Index));

            controller.TempData["Success"].Should().NotBeNull();
            controller.TempData["Success"]!.ToString().Should().Contain("Added to Watchlist");

            watchlistService.Verify(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
            watchlistService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Add_OnFailure_SetsErrorTempData_AndRedirects()
        {
            var (controller, watchlistService, _) =
                TestWatchlistControllerFactory.CreateWatchlistController(userExists: true);

            const int trademarkId = 123;

            watchlistService.Setup(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var actionResult = await controller.Add(trademarkId, null,CancellationToken.None);

            actionResult.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["Error"].Should().NotBeNull();
            controller.TempData["Error"]!.ToString().Should().Contain("Failed to add to Watchlist.");

            watchlistService.Verify(s => s.AddAsync("user-1", trademarkId, It.IsAny<CancellationToken>()), Times.Once);
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
    }
}
