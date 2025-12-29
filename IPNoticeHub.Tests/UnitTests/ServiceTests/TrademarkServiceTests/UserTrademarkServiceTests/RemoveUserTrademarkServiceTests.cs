using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class RemoveUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task RemoveAsync_WhenInCollection_PerformsSoftDeleteOnly()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity1);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            var softDeletedLinks =
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id &&
                x.TrademarkEntityId == entity1.Id).
                ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task RemoveAsync_WhenInCollection_SoftDeletedLinkNotConsideredActive()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity1);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: entity1.Id,
                cancellationToken: default);

            bool isLinkActive = await service.IsInCollectionAsync(
                user.Id,
                entity1.Id,
                includeSoftDeleted: false,
                cancellationToken: default);

            isLinkActive.Should().BeFalse();

            bool linkExists = await service.IsInCollectionAsync(
                user.Id,
                entity1.Id,
                includeSoftDeleted: false,
                cancellationToken: default);

            linkExists.Should().BeFalse();
        }

        [Test]
        public async Task RemoveAsync_WhenNotLinkedInCollection_NoOp()
        {
            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark A",
                owner: "Test Owner A",
                goodsAndServices: "testGoodsAndSerices A",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity1);
            await testDbContext.SaveChangesAsync();

            await service.RemoveAsync(
                user.Id,
                entity1.Id,
                default);

            testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id).
                Should().BeEmpty();
        }
    }
}
