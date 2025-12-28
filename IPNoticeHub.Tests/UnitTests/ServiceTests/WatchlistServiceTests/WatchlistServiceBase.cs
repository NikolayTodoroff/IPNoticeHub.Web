using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests
{
    public class WatchlistServiceBase
    {
        protected Mock<IWatchlistRepository> watchlistRepo = null!;
        protected Mock<ITrademarkStatusSnapshotRepository> snapshotRepo = null!;
        protected Mock<IStatusLabelProvider> statusLabels = null!;
        protected IWatchlistService watchlistService = null!;

        [SetUp]
        public void SetUp()
        {
            watchlistRepo = new Mock<IWatchlistRepository>(MockBehavior.Strict);
            snapshotRepo = new Mock<ITrademarkStatusSnapshotRepository>(MockBehavior.Strict);
            statusLabels = new Mock<IStatusLabelProvider>(MockBehavior.Strict);

            watchlistService = new WatchlistService(
                watchlistRepo.Object,
                snapshotRepo.Object,
                statusLabels.Object);
        }
    }
}
