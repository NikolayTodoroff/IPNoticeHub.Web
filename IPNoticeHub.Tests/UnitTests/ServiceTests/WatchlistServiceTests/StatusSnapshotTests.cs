using FluentAssertions;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using Moq;
using NUnit.Framework;
using IPNoticeHub.Tests.UnitTests.TestFactories;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests
{
    public class StatusSnapshotTests
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
        public async Task GetStatusSnapshotAsync_WhenTrademarkDoesNotExist_ReturnsNull()
        {
            await using var context = InMemoryDbContextFactory.CreateTestDbContext();
            var repo = new TrademarkStatusSnapshotRepository(context);

            var watchlistRepo =
                new Mock<IWatchlistRepository>(MockBehavior.Strict);

            var nonExistingTrademarkId = 999;

            var result = await repo.GetStatusSnapshotAsync(
                nonExistingTrademarkId,
                CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
