using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    [TestFixture]
    public class RemoveUserTrademarkServiceTests
    {
        [Test]
        public async Task RemoveAsync_WhenInCollection_PerformsSoftDeleteOnly()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);
            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = 
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var service = 
                new TrademarkCollectionService(
                    tmRepository, 
                    userTmRepository);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            var softDeletedLinks = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == trademarkEntity.Id).
                ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task RemoveAsync_WhenInCollection_SoftDeletedLinkNotConsideredActive()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);
            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = 
                new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var service = 
                new TrademarkCollectionService(
                    tmRepository, 
                    userTmRepository);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            bool isLinkActive = await service.IsInCollectionAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            isLinkActive.Should().BeFalse();

            bool linkExists = await service.IsInCollectionAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            linkExists.Should().BeFalse();
        }

        [Test]
        public async Task RemoveAsync_WhenNotLinkedInCollection_NoOp()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (tmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.Users.Add(user);
            testDbContext.TrademarkRegistrations.Add(tmEntity);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository =
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository =
                new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(
                tmRepository,
                userTmRepository);

            await service.RemoveAsync(
                user.Id,
                tmEntity.Id,
                default);

            testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id).Should().BeEmpty();
        }
    }
}
