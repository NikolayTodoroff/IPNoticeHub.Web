using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    public class AddUndeleteUserTmRepositoryTests
    {
        [Test]
        public async Task AddOrUndeleteAsync_ShouldInsertNewLink_WhenLinkIsMissing()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Spiritfarer",
                owner: "Obama B.L.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            var userTmLink = await testDbContext.UserTrademarks.
                Include(ut => ut.TrademarkEntity).
                SingleAsync(ut => ut.ApplicationUserId == user.Id && 
                ut.TrademarkEntityId == trademarkEntity.Id);

            userTmLink.IsDeleted.Should().BeFalse();

            userTmLink.DateAdded.Should().
                BeOnOrAfter(DateTime.UtcNow.AddMinutes(-1));
        }

        [Test]
        public async Task AddOrUndeleteAsync_IsIdempotent_WhenAlreadyActive_DoesNotChangeDateAdded()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Swinging Back",
                owner: "The Chosen One",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered, 
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            var userTmLink = 
                await testDbContext.UserTrademarks.SingleAsync(
                    ut => ut.ApplicationUserId == user.Id && 
                ut.TrademarkEntityId == trademarkEntity.Id);

            DateTime initialDateAdded = userTmLink.DateAdded;

            await Task.Delay(50);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            var queryLinksResult = 
                await testDbContext.UserTrademarks.Where(
                    ut => ut.ApplicationUserId == user.Id && 
                ut.TrademarkEntityId == trademarkEntity.Id).
                ToListAsync();

            queryLinksResult.Count.Should().Be(1);
            var singleQueryResult = queryLinksResult.Single();
            singleQueryResult.IsDeleted.Should().BeFalse();
            singleQueryResult.DateAdded.Should().Be(initialDateAdded);
        }

        [Test]
        public async Task AddOrUndeleteAsync_UndeletesAndRefreshesFlagsAndDate()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Chimaira",
                owner: "Default User",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            var userTmLink = 
                await testDbContext.UserTrademarks.SingleAsync(
                    ut => ut.ApplicationUserId == user.Id && 
                ut.TrademarkEntityId == trademarkEntity.Id);

            DateTime initialDateAdded = userTmLink.DateAdded;

            userTmLink.IsDeleted.Should().BeFalse();

            await userTmRepository.SoftRemoveAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            await Task.Delay(50);
 
            await userTmRepository.AddOrUndeleteAsync(
                user.Id, 
                trademarkEntity.Id, 
                CancellationToken.None);

            UserTrademark? undeletedLink = 
                await testDbContext.UserTrademarks.SingleAsync(
                    ut => ut.ApplicationUserId == user.Id && 
                    ut.TrademarkEntityId == trademarkEntity.Id);

            undeletedLink.IsDeleted.Should().BeFalse();
            undeletedLink.DateAdded.Should().BeAfter(initialDateAdded);
        }

    }
}
