using FluentAssertions;
using NUnit.Framework;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    public class IsLinkedAsyncUserTmRepositoryTests
    {
        [Test]
        public async Task IsLinkedAsync_ReturnsFalse_WhenLinkIsMissing()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var linked = await userTmRepository.IsLinkedAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                CancellationToken.None);

            linked.Should().BeFalse();
        }

        [Test]
        public async Task IsLinkedAsync_ReturnsTrue_WhenLinkIsActive()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            bool isLinkedAndActive = 
                await userTmRepository.IsLinkedAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                CancellationToken.None);

            isLinkedAndActive.Should().BeTrue();
        }

        [Test]
        public async Task IsLinkedAsync_Respects_SoftDeletedFlag()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZEN",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new 
                UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);
            
            await userTmRepository.SoftRemoveAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            var defaultCheck = 
                await userTmRepository.IsLinkedAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                CancellationToken.None);
            
            var includeDeletedCheck = 
                await userTmRepository.IsLinkedAsync(
                    user.Id, 
                    trademarkEntity.Id, 
                    includeSoftDeleted: true, 
                    CancellationToken.None);

            defaultCheck.Should().BeFalse(
                "soft-deleted link should not count by default");
            
            includeDeletedCheck.Should().BeTrue(
                "includeSoftDeleted=true should count soft-deleted links");
        }
    }
}
