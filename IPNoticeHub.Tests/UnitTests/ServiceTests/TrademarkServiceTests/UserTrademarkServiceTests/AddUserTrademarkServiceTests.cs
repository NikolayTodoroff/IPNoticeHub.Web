using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class AddUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task AddAsync_WhenNotInCollection_AddsLink_ThenIsInCollectionReturnsTrue()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            bool isInCollection = await service.IsInCollectionAsync(
                user.Id,
                tmEntity1.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            isInCollection.Should().BeTrue();

            var links = 
                testDbContext.UserTrademarks.Where
                (x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == tmEntity1.Id).
                ToList();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddAsync_WhenPreviouslySoftDeleted_UndeletesExistingLink()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            var userTrademark = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == tmEntity1.Id).
                ToList();

            userTrademark.Should().HaveCount(1);
            userTrademark[0].IsDeleted.Should().BeTrue();

            await service.AddAsync(
               userId: user.Id,
               trademarkId: tmEntity1.Id,
               cancellationToken: default);

            var links = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == tmEntity1.Id).
                ToList();

            links.Should().HaveCount(1);
            links[index: 0].IsDeleted.Should().BeFalse();

            var isInCollection = await service.IsInCollectionAsync(
                user.Id, 
                tmEntity1.Id, 
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
            await service.AddAsync(
                user.Id,
                tmEntity1.Id,
                default);

            await service.AddAsync(
                user.Id,
                tmEntity1.Id,
                default);

            var links = testDbContext.UserTrademarks.
                Where(x => x.ApplicationUserId == user.Id &&
                x.TrademarkEntityId == tmEntity1.Id).
                ToList();

            links.Should().HaveCount(1);
            links[0].IsDeleted.Should().BeFalse();
        }
    }
}
