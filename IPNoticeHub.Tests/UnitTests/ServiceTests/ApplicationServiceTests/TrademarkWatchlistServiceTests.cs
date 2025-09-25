using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

using IPNoticeHub.Services.Application.Implementations;
using IPNoticeHub.Data.Repositories.Application.Abstractions;
using IPNoticeHub.Services.Application.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.ApplicationServiceTests
{
    [TestFixture]
    public class TrademarkWatchlistServiceTests
    {
        [Test]
        public async Task AddAsync_WhenSnapshotExists_CallsRepoWithSnapshot()
        {
            var userId = "user-1";
            var trademarkId = 123;

            var snapshot = (StatusCodeRaw: (int?)630,
                            StatusDetail: "New Application-Record initialized not assigned to examiner",
                            StatusDateUtc: (DateTime?)new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var watchlistRepo = new Mock<IUserTrademarkWatchlistRepository>(MockBehavior.Strict);
            var snapshotRepo = new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Strict);
            var labels = new Mock<IStatusLabelProvider>(MockBehavior.Loose);

            snapshotRepo
                .Setup(r => r.GetStatusSnapshotAsync(trademarkId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(snapshot);

            watchlistRepo
                .Setup(r => r.AddOrUndeleteAsync(userId, trademarkId, snapshot.StatusCodeRaw,
                                                 snapshot.StatusDetail, snapshot.StatusDateUtc,
                                                 It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var watchlistService = new TrademarkWatchlistService(watchlistRepo.Object, snapshotRepo.Object, labels.Object);

            await watchlistService.AddAsync(userId, trademarkId, CancellationToken.None);

            snapshotRepo.Verify(r => r.GetStatusSnapshotAsync(trademarkId, It.IsAny<CancellationToken>()), Times.Once);

            watchlistRepo.Verify(r => r.AddOrUndeleteAsync(userId, trademarkId, snapshot.StatusCodeRaw,
                                                           snapshot.StatusDetail, snapshot.StatusDateUtc,
                                                           It.IsAny<CancellationToken>()), Times.Once);

            watchlistRepo.VerifyNoOtherCalls();
            snapshotRepo.VerifyNoOtherCalls();
        }
    }
}
