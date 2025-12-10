using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    /// <summary>
    /// Verifies that repository methods respect a pre-cancelled CancellationToken 
    /// and propagate OperationCanceledException or TaskCanceledException.
    /// Ensures database operations halt promptly during upstream cancellations.
    /// </summary>
    [TestFixture]
    public class UserTmRepoCancellationTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_ThrowsException_WhenCancellationRequested()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Moon Apes",
                owner: "Default Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);

            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await FluentActions.Awaiting(() => userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id, cancellationTokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task SoftRemoveAsync_Throws_WhenCancellationRequested()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Moon Apes",
                owner: "Default Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);

            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user.Id, trademarkEntity.Id);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await FluentActions.Awaiting(() => userTmRepository.SoftRemoveAsync(user.Id, trademarkEntity.Id, cancellationTokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
