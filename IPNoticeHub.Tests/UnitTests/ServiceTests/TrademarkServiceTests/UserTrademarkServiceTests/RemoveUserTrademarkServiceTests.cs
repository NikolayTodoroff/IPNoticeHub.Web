using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class RemoveUserTrademarkServiceTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task RemoveAsync_WhenInCollection_PerformsSoftDeleteOnly()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            var softDeletedLinks = 
                testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id && 
                x.TrademarkEntityId == tmEntity1.Id).
                ToList();

            softDeletedLinks.Should().HaveCount(1);
            softDeletedLinks[0].IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task RemoveAsync_WhenInCollection_SoftDeletedLinkNotConsideredActive()
        {
            await service.AddAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            await service.RemoveAsync(
                userId: user.Id,
                trademarkId: tmEntity1.Id,
                cancellationToken: default);

            bool isLinkActive = await service.IsInCollectionAsync(
                user.Id,
                tmEntity1.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            isLinkActive.Should().BeFalse();

            bool linkExists = await service.IsInCollectionAsync(
                user.Id,
                tmEntity1.Id, 
                includeSoftDeleted: false, 
                cancellationToken: default);

            linkExists.Should().BeFalse();
        }

        [Test]
        public async Task RemoveAsync_WhenNotLinkedInCollection_NoOp()
        {
            await service.RemoveAsync(
                user.Id,
                tmEntity1.Id,
                default);

            testDbContext.UserTrademarks.Where(
                x => x.ApplicationUserId == user.Id).
                Should().BeEmpty();
        }
    }
}
