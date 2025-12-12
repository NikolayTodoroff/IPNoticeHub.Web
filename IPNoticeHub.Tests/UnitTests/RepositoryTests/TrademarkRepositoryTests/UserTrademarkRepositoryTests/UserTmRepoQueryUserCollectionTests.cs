using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    [TestFixture]
    public class UserTmRepoQueryUserCollectionTests
    {
        [Test]
        public async Task QueryUserCollection_ReturnsActiveTrademarksForUser_WithClasses_AsNoTracking()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = InMemoryDbContextFactory.CreateApplicationUser(id: "user1");
            var user2 = InMemoryDbContextFactory.CreateApplicationUser(id: "user2");

            testDbContext.Users.AddRange(user1, user2);

            var (user1TmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Existing One",
                owner: "Owner1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (user1TmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Removed One",
                owner: "Owner1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2143657",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            var (user2TmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "From Another User",
                owner: "Owner2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 18 });

            testDbContext.TrademarkRegistrations.AddRange(
                user1TmEntity1,
                user1TmEntity2,
                user2TmEntity);

            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            // user1 -> two links, second is soft-removed
            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity1.Id);
            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity2.Id);
            await userTmRepository.SoftRemoveAsync(user1.Id, user1TmEntity2.Id);

            // user2 -> one link (should not appear for user1)
            await userTmRepository.AddOrUndeleteAsync(user2.Id, user2TmEntity.Id);

            var pagedResult = await userTmRepository.GetUserCollectionPageAsync(
                user1.Id,
                CollectionSortBy.DateAddedDesc,
                currentPage: 1,
                resultsPerPage: 20,
                cancellationToken: default);

            var queryResult = pagedResult.Results;

            // still only one active link for user1
            queryResult.Should().HaveCount(1);

            var link = queryResult.Single();

            link.IsDeleted.Should().BeFalse();
            link.TrademarkEntity.Should().NotBeNull();
            link.TrademarkEntity!.Wordmark.Should().Be("The Existing One");

            link.TrademarkEntity.Classes!
                .Select(c => c.ClassNumber)
                .Should().BeEquivalentTo(new[] { 9, 25 });

            // AsNoTracking – link + entity are detached
            testDbContext.Entry(link).State.Should().Be(EntityState.Detached);
            testDbContext.Entry(link.TrademarkEntity!).State.Should().Be(EntityState.Detached);
        }


        [Test]
        public async Task QueryUserLinks_ReturnsActiveUserLinks_WithTrademarkDetailsAndClasses_AsNoTracking()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            var user2 = 
                InMemoryDbContextFactory.CreateApplicationUser("user2");

            testDbContext.Users.AddRange(
                user1, 
                user2);

            var (user1TmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Existing One",
                owner: "Owner1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (user1TmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Removed One",
                owner: "Owner1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2143657",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            var (user2TmEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "From Another User",
                owner: "Owner2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered, 
                DataProvider.USPTO, 
                classNumbers: new[] { 18 });

            testDbContext.TrademarkRegistrations.AddRange(
                user1TmEntity1, 
                user1TmEntity2, 
                user2TmEntity);
            
            await testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(
                user1.Id, 
                user1TmEntity1.Id);

            await userTmRepository.AddOrUndeleteAsync(
                user1.Id, 
                user1TmEntity2.Id);

            await userTmRepository.SoftRemoveAsync(
                user1.Id, 
                user1TmEntity2.Id);

            await userTmRepository.AddOrUndeleteAsync(
                user2.Id, 
                user2TmEntity.Id);

            var queryLinksResult = 
                userTmRepository.
                QueryUserLinks(user1.Id).
                ToList();

            queryLinksResult.Should().
                HaveCount(1);

            var singleLink = queryLinksResult.Single();

            singleLink.TrademarkEntity!.Wordmark.Should().
                Be("The Existing One");

            singleLink.TrademarkEntity!.Classes!.Select
                (c => c.ClassNumber).Should().
                BeEquivalentTo(new[] { 9, 25 });

            testDbContext.Entry(singleLink).State.Should().
                Be(EntityState.Detached);

            testDbContext.Entry(singleLink.TrademarkEntity!).State.Should().
                Be(EntityState.Detached);
        }
    }
}
