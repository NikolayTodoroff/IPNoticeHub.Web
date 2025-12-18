using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    [TestFixture]
    public class UserTmRepoCancellationTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_ThrowsException_WhenCancellationRequested()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
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

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id);

            using var cancellationTokenSource = 
                new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            await FluentActions.Awaiting(() => 
            userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                cancellationTokenSource.Token)).
                Should().
                ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task SoftRemoveAsync_Throws_WhenCancellationRequested()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
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

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id);

            using var cancellationTokenSource = 
                new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            await FluentActions.Awaiting(() => 
            userTmRepository.SoftRemoveAsync(
                user.Id, 
                trademarkEntity.Id, 
                cancellationTokenSource.Token)).Should().
                ThrowAsync<OperationCanceledException>();
        }
    }
}
