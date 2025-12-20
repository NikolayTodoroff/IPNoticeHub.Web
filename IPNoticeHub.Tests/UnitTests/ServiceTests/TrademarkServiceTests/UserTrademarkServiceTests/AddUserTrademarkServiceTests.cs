using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    [TestFixture]
    public class AddUserTrademarkServiceTests
    {
        [Test]
        public async Task AddAsync_WhenNotInCollection_AddsLink_ThenIsInCollectionReturnsTrue()
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

            var service = new TrademarkCollectionService(
                tmRepository, 
                userTmRepository);

            await service.AddAsync(
                userId: user.Id,
                trademarkId: trademarkEntity.Id,
                cancellationToken: default);

            bool linkedInCollection = await service.IsInCollectionAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            linkedInCollection.Should().BeTrue();

            var existingLink = testDbContext.UserTrademarks.Where
                (x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == trademarkEntity.Id).
                ToList();

            existingLink.Should().HaveCount(1);
            existingLink[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddAsync_WhenPreviouslySoftDeleted_UndeletesExistingLink()
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

            var service = new TrademarkCollectionService(
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

            var softDeletedLinks = testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == trademarkEntity.Id).
                ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();

            await service.AddAsync(
               userId: user.Id,
               trademarkId: trademarkEntity.Id,
               cancellationToken: default);

            var unDeletedLinks = testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == trademarkEntity.Id).
                ToList();

            unDeletedLinks.Should().HaveCount(1);
            unDeletedLinks[index: 0].IsDeleted.Should().BeFalse();

            var linkedInCollection = await service.IsInCollectionAsync(
                user.Id, 
                trademarkEntity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            linkedInCollection.Should().BeTrue();
        }

        [Test]
        public async Task AddAsync_WhenTrademarkIdDoesNotExist_DoesNothing()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository =
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository =
                new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(
                tmRepository,
                userTmRepository);

            await service.AddAsync(
                user.Id,
                1234567,
                default);

            testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id).Should().BeEmpty();
        }

        [Test]
        public async Task AddAsync_WhenAlreadyLinkedInCollection_DoesNotCreateDuplicateRow()
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

            ITrademarkRepository tmRepository = new
                TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository =
                new UserTrademarkRepository(testDbContext);

            var service = new TrademarkCollectionService(tmRepository, userTmRepository);

            await service.AddAsync(
                user.Id,
                tmEntity.Id,
                default);

            await service.AddAsync(
                user.Id,
                tmEntity.Id,
                default);

            var links = testDbContext.UserTrademarks.
                Where(x => x.ApplicationUserId == user.Id &&
                x.TrademarkEntityId == tmEntity.Id).
                ToList();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }
    }
}
