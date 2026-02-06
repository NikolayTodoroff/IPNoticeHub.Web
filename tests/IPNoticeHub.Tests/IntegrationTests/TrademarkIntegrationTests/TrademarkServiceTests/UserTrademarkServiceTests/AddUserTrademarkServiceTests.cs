using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.UserTrademarkServiceTests
{
    public class AddUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task AddAsync_WhenNotInCollection_AddsLink_ThenIsInCollectionReturnsTrue()
        {
            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity.Id,
                cancellationToken: default);

            bool isInCollection = await service.IsInCollectionAsync(
                user.Id,
                entity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            isInCollection.Should().BeTrue();

            var userTrademark = 
                testDbContext.UserTrademarks.Where(
                    x => x.ApplicationUserId == user.Id && 
                    x.TrademarkEntityId == entity.Id).
                    ToList();

            userTrademark.Should().HaveCount(1);
            userTrademark[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddAsync_WhenPreviouslySoftDeleted_UndeletesExistingLink()
        {
            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: entity.Id,
                cancellationToken: default);

            var userTrademark = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == entity.Id).
                ToList();

            userTrademark.Should().HaveCount(1);
            userTrademark[0].IsDeleted.Should().BeTrue();

            await service.AddAsync(
               userId: user.Id,
               trademarkId: entity.Id,
               cancellationToken: default);

            var links = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == entity.Id).
                ToList();

            links.Should().HaveCount(1);
            links[index: 0].IsDeleted.Should().BeFalse();

            var isInCollection = await service.IsInCollectionAsync(
                user.Id,
                entity.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            isInCollection.Should().BeTrue();
        }

        [Test]
        public async Task AddAsync_WhenTrademarkIdDoesNotExist_DoesNothing()
        {
            await service.AddAsync(
                user.Id,
                1234567,
                default);

            testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id).
                Should().BeEmpty();
        }

        [Test]
        public async Task AddAsync_WhenAlreadyLinkedInCollection_DoesNotCreateDuplicateRow()
        {
            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                user.Id,
                entity.Id,
                default);

            await service.AddAsync(
                user.Id,
                entity.Id,
                default);

            var links = testDbContext.UserTrademarks.
                Where(x => x.ApplicationUserId == user.Id &&
                x.TrademarkEntityId == entity.Id).
                ToList();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }
    }
}
