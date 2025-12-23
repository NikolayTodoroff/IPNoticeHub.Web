using FluentAssertions;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests
{
    public class WatchlistServiceTests : WatchlistServiceBase
    {
        [Test]
        public async Task GetListByUserAsync_MapsItems_ComputesStatusChangeByCode_UsesLabelFallback()
        {
            var userId = "user-1";

            statusLabels.Setup(
                l => l.GetStatusLabel(
                    "USPTO", 
                    700)).
                    Returns("Registered");

            var linkA = new Watchlist
            {
                UserId = userId,
                TrademarkId = 101,
                NotificationsEnabled = false,
                AddedOnUtc = new DateTime(
                    2025, 
                    1, 
                    5, 
                    12, 
                    0, 
                    0, 
                    DateTimeKind.Utc),
                InitialStatusCodeRaw = 630,
                InitialStatusText = "New Application",
                InitialStatusDateUtc = new DateTime(
                    2025, 
                    1, 
                    1, 
                    0, 
                    0, 
                    0, 
                    DateTimeKind.Utc),
                Trademark = new TrademarkEntity
                {
                    Id = 101,
                    RegistrationNumber = "REG-101",
                    Wordmark = "WM1",
                    Owner = "Owner A",
                    StatusCodeRaw = 700,
                    StatusDetail = "Registered"
                }
            };

            var linkB = new Watchlist
            {
                UserId = userId,
                TrademarkId = 202,
                NotificationsEnabled = true,
                AddedOnUtc = new DateTime(
                    2025, 
                    1, 
                    4, 
                    12, 
                    0, 
                    0, 
                    DateTimeKind.Utc),
                InitialStatusCodeRaw = 700,
                InitialStatusText = "Registered",
                InitialStatusDateUtc = new DateTime(
                    2024, 
                    12, 
                    31, 
                    0, 
                    0, 
                    0, 
                    DateTimeKind.Utc),
                Trademark = new TrademarkEntity
                {
                    Id = 202,
                    RegistrationNumber = null,
                    Wordmark = "WM2",
                    Owner = "Owner B",
                    StatusCodeRaw = 700,
                    StatusDetail = ""
                }
            };

            watchlistRepo.Setup(
                r => r.ListByUserAsync(
                    userId, 
                    0, 
                    200, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<Watchlist> { linkA, linkB });

            var items = 
                await watchlistService.GetListByUserAsync(
                    userId, 
                    CancellationToken.None);

            items.Should().HaveCount(2);
            var itemA = items[0];
            itemA.Id.Should().Be(101);
            itemA.RegistrationNumber.Should().Be("REG-101");
            itemA.Wordmark.Should().Be("WM1");
            itemA.Owner.Should().Be("Owner A");
            itemA.InitialStatus.Should().Be("New Application");
            itemA.CurrentStatus.Should().Be("Registered");
            itemA.HasStatusChange.Should().BeTrue();
            itemA.NotificationsEnabled.Should().BeFalse();

            var itemB = items[1];

            itemB.Id.Should().Be(202);
            itemB.RegistrationNumber.Should().Be("");
            itemB.Wordmark.Should().Be("WM2");
            itemB.Owner.Should().Be("Owner B");
            itemB.InitialStatus.Should().Be("Registered");
            itemB.CurrentStatus.Should().Be("Registered");
            itemB.HasStatusChange.Should().BeFalse();

            statusLabels.Verify(
                l => l.GetStatusLabel(
                    "USPTO", 
                    700), 
                Times.Once);

            watchlistRepo.Verify(
                r => r.ListByUserAsync(
                    userId, 
                    0, 
                    200, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
            statusLabels.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetListByUserAsync_WhenCodesNull_ComputesChangeByNormalizedText()
        {
            var userId = "user-1";

            var linkA = new Watchlist
            {
                UserId = userId,
                TrademarkId = 11,
                AddedOnUtc = DateTime.UtcNow,
                InitialStatusCodeRaw = null,
                InitialStatusText = "  Live   Registered  ",
                Trademark = new TrademarkEntity
                {
                    Id = 11,
                    RegistrationNumber = "RN-11",
                    Wordmark = "MARK A",
                    Owner = "Owner A",
                    StatusCodeRaw = null,
                    StatusDetail = "live registered"
                }
            };

            var linkB = new Watchlist
            {
                UserId = userId,
                TrademarkId = 22,
                AddedOnUtc = DateTime.UtcNow.AddMinutes(-1),
                InitialStatusCodeRaw = null,
                InitialStatusText = "Abandoned — Express",
                Trademark = new TrademarkEntity
                {
                    Id = 22,
                    RegistrationNumber = "RN-22",
                    Wordmark = "MARK B",
                    Owner = "Owner B",
                    StatusCodeRaw = null,
                    StatusDetail = "abandoned express" 
                }
            };

            watchlistRepo.Setup(
                r => r.ListByUserAsync(
                    userId, 
                    0, 
                    200, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(new List<Watchlist> { linkA, linkB });

            var items = 
                await watchlistService.GetListByUserAsync(
                    userId, 
                    CancellationToken.None);

            items.Should().HaveCount(2);

            var itemA = items[0];

            itemA.Id.Should().Be(11);
            itemA.InitialStatus.Should().Be("  Live   Registered  ");
            itemA.CurrentStatus.Should().Be("live registered");
            itemA.HasStatusChange.Should().BeFalse();

            var itemB = items[1];

            itemB.Id.Should().Be(22);
            itemB.InitialStatus.Should().Be("Abandoned — Express");
            itemB.CurrentStatus.Should().Be("abandoned express");
            itemB.HasStatusChange.Should().BeTrue();

            statusLabels.VerifyNoOtherCalls();

            watchlistRepo.Verify(
                r => r.ListByUserAsync(
                    userId, 
                    0, 
                    200, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ExistsAsync_WhenTrademarkExistsInRepository_ReturnsTrue()
        {
            const string userId = "user-1";
            const int trademarkId = 321;

            watchlistRepo.Setup(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync(true);

            var watchlistLinkExist = 
                await watchlistService.ExistsAsync(
                    userId, 
                    trademarkId, 
                    CancellationToken.None);

            watchlistLinkExist.Should().BeTrue();

            watchlistRepo.Verify(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ExistsAsync_WhenTrademarkDoesNotExistInRepository_ReturnsFalse()
        {
            const string userId = "user-2";
            const int trademarkId = 999;

            watchlistRepo.Setup(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            var watchlistLinkExist = 
                await watchlistService.ExistsAsync(
                    userId, 
                    trademarkId, 
                    CancellationToken.None);

            watchlistLinkExist.Should().BeFalse();

            watchlistRepo.Verify(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RemoveAsync_CallsRepositorySoftRemove()
        {
            const string userId = "user-1";
            const int trademarkId = 123;

            watchlistRepo.Setup(
                r => r.SoftRemoveAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            await watchlistService.RemoveAsync(
                userId, 
                trademarkId, 
                CancellationToken.None);

            watchlistRepo.Verify(
                r => r.SoftRemoveAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ToggleNotificationsAsync_CallsRepositoryWithCorrectParameters()
        {
            const string userId = "user-2";
            const int trademarkId = 456;
            const bool notificationsEnabled = true;

            watchlistRepo.Setup(
                r => r.ToggleNotificationsAsync(
                    userId, 
                    trademarkId, 
                    notificationsEnabled, 
                    It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            await watchlistService.ToggleNotificationsAsync(
                userId, 
                trademarkId, 
                notificationsEnabled, 
                CancellationToken.None);

            watchlistRepo.Verify(
                r => r.ToggleNotificationsAsync(
                    userId, 
                    trademarkId, 
                    notificationsEnabled, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ComputeStatusChange_WhenBothTextsEmptyString_ReturnsFalse()
        {
            var userId = "user-1";

            var link = new Watchlist
            {
                UserId = userId,
                TrademarkId = 1,
                AddedOnUtc = DateTime.UtcNow,
                InitialStatusCodeRaw = null,
                InitialStatusText = "",
                Trademark = new TrademarkEntity
                {
                    Id = 1,
                    RegistrationNumber = "RN-1",
                    Wordmark = "TEST",
                    Owner = "Owner",
                    StatusCodeRaw = null,
                    StatusDetail = ""
                }
            };

            watchlistRepo.Setup(
                r => r.ListByUserAsync(
                    userId,
                    0,
                    200,
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(new List<Watchlist> { link });

            var items =
                await watchlistService.GetListByUserAsync(
                    userId,
                    CancellationToken.None);

            items[0].HasStatusChange.Should().BeFalse();
        }
    }
}
