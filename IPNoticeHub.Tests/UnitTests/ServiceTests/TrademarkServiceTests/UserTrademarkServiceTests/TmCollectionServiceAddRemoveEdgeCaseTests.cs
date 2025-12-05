using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    /// <summary>
    /// Section: TrademarkCollectionService – AddAsync and RemoveAsync Edge Cases
    /// - Ensures AddAsync does not create a link when the provided trademark entity Id is unknown.
    /// - Verifies AddAsync prevents duplicate entries when the link already exists and is active.
    /// - Confirms RemoveAsync performs no operation when the trademark is not part of the user's collection.
    /// </summary>
    [TestFixture]
    public class TmCollectionServiceAddRemoveEdgeCaseTests
    {
        [Test]
        public async Task AddAsync_WhenTrademarkIdDoesNotExist_DoesNothing()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.AddAsync(user.Id, 1234567, default);

            testDbContext.UserTrademarks.Where(x => x.UserId == user.Id).Should().BeEmpty();
        }

        [Test]
        public async Task AddAsync_WhenAlreadyLinkedInCollection_DoesNotCreateDuplicateRow()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

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

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.AddAsync(user.Id, tmEntity.Id, default);
            await service.AddAsync(user.Id, tmEntity.Id, default);

            var links = testDbContext.UserTrademarks
                          .Where(x => x.UserId == user.Id && x.TrademarkId == tmEntity.Id)
                          .ToList();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task RemoveAsync_WhenNotLinkedInCollection_NoOp()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

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

            ITrademarkRepository tmRepository = new TrademarkRepository(testDbContext);
            IUserTrademarkRepository userTmRepository = new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.RemoveAsync(user.Id, tmEntity.Id, default);

            testDbContext.UserTrademarks.Where(x => x.UserId == user.Id).Should().BeEmpty();
        }
    }
}
