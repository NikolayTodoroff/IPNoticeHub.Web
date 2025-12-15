using FluentAssertions;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Domain.Entities.Watchlist;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests
{
    [TestFixture]
    public class TrademarkWatchlistServiceTests
    {
        [Test]
        public async Task AddAsync_WhenSnapshotExists_CallsRepoWithSnapshot()
        {
            var userId = "user-1";
            var trademarkId = 123;

            var snapshot = (
                StatusCodeRaw: (int?)630,
                StatusDetail: "New Application-Record initialized not assigned to examiner",
                StatusDateUtc: (
                DateTime?)new DateTime(
                    2025, 
                    1, 
                    1, 
                    0, 
                    0, 
                    0, 
                    DateTimeKind.Utc));

            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Strict);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            snapshotRepo.Setup(
                sr => sr.GetStatusSnapshotAsync(
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(snapshot);

            watchlistRepo.Setup(
                wr => wr.AddOrUndeleteAsync(
                    userId, 
                    trademarkId, 
                    snapshot.StatusCodeRaw,
                    snapshot.StatusDetail, 
                    snapshot.StatusDateUtc,
                    It.IsAny<CancellationToken>())).
                    Returns(Task.CompletedTask);

            var watchlistService = 
                new WatchlistService(
                    watchlistRepo.Object, 
                    snapshotRepo.Object, 
                    statusLabels.Object);

            await watchlistService.AddAsync(
                userId, 
                trademarkId, 
                CancellationToken.None);

            snapshotRepo.Verify(
                snapshotRepo => snapshotRepo.GetStatusSnapshotAsync(
                    trademarkId, 
                    It.IsAny<CancellationToken>()), Times.Once);

            watchlistRepo.Verify(
                wr => wr.AddOrUndeleteAsync(
                    userId, 
                    trademarkId, 
                    snapshot.StatusCodeRaw,
                    snapshot.StatusDetail, 
                    snapshot.StatusDateUtc,
                    It.IsAny<CancellationToken>()), Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
            snapshotRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task AddAsync_WithoutSnapshot_ThrowsInvalidOperationException()
        {
            var userId = "user-1";
            var trademarkId = 123;

            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Strict);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            snapshotRepo.Setup(
                s => s.GetStatusSnapshotAsync(
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(((
                int? StatusCodeRaw, 
                string StatusDetail, 
                DateTime? StatusDateUtc)?)null);

            var watchlistService = new WatchlistService(
                watchlistRepo.Object, 
                snapshotRepo.Object, 
                statusLabels.Object);


            await watchlistService.Awaiting(ws => ws.AddAsync(
                userId, 
                trademarkId, 
                CancellationToken.None)).
                Should().
                ThrowAsync<InvalidOperationException>().
                WithMessage($"*{trademarkId}*");

            watchlistRepo.Verify(r => r.AddOrUndeleteAsync(
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<int?>(),
                It.IsAny<string>(), 
                It.IsAny<DateTime?>(), 
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task AddAsync_WhenSnapshotPresent_CallsRepoWithSnapshotValues()
        {
            const string userId = "user-1";
            const int trademarkId = 123;

            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Strict);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            int? snapshotCode = 700;
            string snapshotText = "Registered";

            DateTime snapshotDate = new DateTime(
                2023, 
                05, 
                01, 
                12, 
                00, 
                00, 
                DateTimeKind.Utc);


            snapshotRepo.Setup(
                s => s.GetStatusSnapshotAsync(
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                    ReturnsAsync((
                    snapshotCode, 
                    snapshotText, 
                    snapshotDate));

            watchlistRepo.Setup(
                r => r.AddOrUndeleteAsync(
                    userId,
                    trademarkId,
                    snapshotCode,
                    snapshotText,
                    snapshotDate,
                    It.IsAny<CancellationToken>())).
                    Returns(Task.CompletedTask);

            var watchlistService = new WatchlistService(
                watchlistRepo.Object, 
                snapshotRepo.Object, 
                statusLabels.Object);

            await watchlistService.AddAsync(
                userId, 
                trademarkId, CancellationToken.None);


            snapshotRepo.Verify(
                sr => sr.GetStatusSnapshotAsync(
                    trademarkId, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.Verify(
                wr => wr.AddOrUndeleteAsync(
                    userId, 
                    trademarkId, 
                    snapshotCode,
                    snapshotText, 
                    snapshotDate, 
                    It.IsAny<CancellationToken>()), 
                Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
            snapshotRepo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetListByUserAsync_MapsItems_ComputesStatusChangeByCode_UsesLabelFallback()
        {
            var userId = "user-1";

            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Strict);

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

            var watchlistService = 
                new WatchlistService(
                    watchlistRepo.Object,
                    snapshotRepo.Object, 
                    statusLabels.Object);

            var items = 
                await watchlistService.GetListByUserAsync(
                    userId, 
                    CancellationToken.None);

            items.Should().
                HaveCount(2);

            var itemA = items[0];

            itemA.Id.Should().
                Be(101);

            itemA.RegistrationNumber.Should().
                Be("REG-101");

            itemA.Wordmark.Should().
                Be("WM1");

            itemA.Owner.Should().
                Be("Owner A");

            itemA.InitialStatus.Should().
                Be("New Application");

            itemA.CurrentStatus.Should().
                Be("Registered");

            itemA.HasStatusChange.Should().
                BeTrue();

            itemA.NotificationsEnabled.Should().
                BeFalse();

            var itemB = items[1];

            itemB.Id.Should().
                Be(202);

            itemB.RegistrationNumber.Should().
                Be("");

            itemB.Wordmark.Should().
                Be("WM2");

            itemB.Owner.Should().
                Be("Owner B");

            itemB.InitialStatus.Should().
                Be("Registered");

            itemB.CurrentStatus.Should().
                Be("Registered");

            itemB.HasStatusChange.Should().
                BeFalse();

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
            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Strict);

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

            var watchlistService = 
                new WatchlistService(
                    watchlistRepo.Object,
                    snapshotRepo.Object,
                    statusLabels.Object);

            var items = 
                await watchlistService.GetListByUserAsync(
                    userId, 
                    CancellationToken.None);

            items.Should().
                HaveCount(2);

            var itemA = items[0];

            itemA.Id.Should().
                Be(11);
            itemA.InitialStatus.Should().
                Be("  Live   Registered  ");

            itemA.CurrentStatus.Should().
                Be("live registered");

            itemA.HasStatusChange.Should().
                BeFalse();

            var itemB = items[1];

            itemB.Id.Should().
                Be(22);

            itemB.InitialStatus.Should().
                Be("Abandoned — Express");

            itemB.CurrentStatus.Should().
                Be("abandoned express");

            itemB.HasStatusChange.Should().
                BeTrue();

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
            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            const string userId = "user-1";
            const int trademarkId = 321;

            watchlistRepo.Setup(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var watchlistService = 
                new WatchlistService(
                    watchlistRepo.Object, 
                    snapshotRepo.Object, 
                    statusLabels.Object);

            var watchlistLinkExist = 
                await watchlistService.ExistsAsync(
                    userId, 
                    trademarkId, 
                    CancellationToken.None);

            watchlistLinkExist.Should().
                BeTrue();

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
            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            const string userId = "user-2";
            const int trademarkId = 999;

            watchlistRepo.Setup(
                r => r.ExistsAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                ReturnsAsync(false);

            var service = 
                new WatchlistService(
                    watchlistRepo.Object, 
                    snapshotRepo.Object, 
                    statusLabels.Object);

            var watchlistLinkExist = 
                await service.ExistsAsync(
                    userId, 
                    trademarkId, 
                    CancellationToken.None);

            watchlistLinkExist.Should().
                BeFalse();

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
            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabel = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            const string userId = "user-1";
            const int trademarkId = 123;

            watchlistRepo.Setup(
                r => r.SoftRemoveAsync(
                    userId, 
                    trademarkId, 
                    It.IsAny<CancellationToken>())).
                Returns(Task.CompletedTask);

            var watchlistService = 
                new WatchlistService(
                    watchlistRepo.Object, 
                    snapshotRepo.Object, 
                    statusLabel.Object);

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
            var watchlistRepo = 
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo = 
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels = 
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

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

            var service = 
                new WatchlistService(
                    watchlistRepo.Object, 
                    snapshotRepo.Object, 
                    statusLabels.Object);

            await service.ToggleNotificationsAsync(
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
            var watchlistRepo =
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var snapshotRepo =
                new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Loose);

            var statusLabels =
                new Mock<IStatusLabelProvider>(MockBehavior.Loose);

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

            var watchlistService =
                new WatchlistService(
                    watchlistRepo.Object,
                    snapshotRepo.Object,
                    statusLabels.Object);

            var items =
                await watchlistService.GetListByUserAsync(
                    userId,
                    CancellationToken.None);

            items[0].HasStatusChange.Should().
                BeFalse();
        }
    }
}
