using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    /// <summary>
    /// Section: QueryUserCollection shape (trademark projection)
    /// This method ensures that collection pages display trademarks along with their associated classes
    /// while maintaining correctness and performance. It avoids pulling soft-deleted, foreign items,
    /// or tracking entities that could degrade accuracy or efficiency.
    /// - Returns only active links for the specified user (excludes soft-deleted and other users' data).
    /// - Includes TrademarkRegistration and its associated Classes.
    /// - Uses AsNoTracking to ensure returned entities are detached from the DbContext.
    /// - Supports screens and jobs requiring metadata like DateAdded and AddedToWatchlist alongside trademarks.
    /// </summary>
    [TestFixture]
    public class UserTmRepoQueryUserCollectionTests
    {
        [Test]
        public async Task QueryUserCollection_ReturnsActiveTrademarksForUser_WithClasses_AsNoTracking()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = InMemoryDbContextFactory.CreateApplicationUser("user1");
            var user2 = InMemoryDbContextFactory.CreateApplicationUser("user2");

            testDbContext.Users.AddRange(user1, user2);

            var (user1TmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Existing One",
                owner: "Owner1",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO, 
                classNumbers: new[] { 9, 25 });

            var (user1TmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Removed One",
                owner: "Owner1",
                regNumber: "2143657",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO, 
                classNumbers: new[] { 30 });

            var (user2TmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "From Another User",
                owner: "Owner2",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO, 
                classNumbers: new[] { 18 });

            testDbContext.TrademarkRegistrations.AddRange(user1TmEntity1,user1TmEntity2,user2TmEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity1.Id);
            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity2.Id);
            await userTmRepository.SoftRemoveAsync(user1.Id, user1TmEntity2.Id);

            await userTmRepository.AddOrUndeleteAsync(user2.Id, user2TmEntity.Id);

            var queryResult = userTmRepository.QueryUserCollection(user1.Id).ToList();

            // Asserts that only "The Existing One" remains for user1, "The Removed One" was soft-deleted, "From Another User" belongs to user2
            queryResult.Select(r => r.Wordmark).Should().Equal("The Existing One");

            // Asserts that the query result includes the correct trademark classes
            queryResult.Single().Classes!.Select(c => c.ClassNumber).Should().BeEquivalentTo(new[] { 9, 25 });

            // Verifies that the entity is detached from the DbContext, ensuring AsNoTracking behavior
            testDbContext.Entry(queryResult.Single()).State.Should().Be(EntityState.Detached);
        }

        [Test]
        public async Task QueryUserLinks_ReturnsActiveUserLinks_WithTrademarkDetailsAndClasses_AsNoTracking()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user1 = InMemoryDbContextFactory.CreateApplicationUser("user1");
            var user2 = InMemoryDbContextFactory.CreateApplicationUser("user2");

            testDbContext.Users.AddRange(user1, user2);

            var (user1TmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Existing One",
                owner: "Owner1",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (user1TmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "The Removed One",
                owner: "Owner1",
                regNumber: "2143657",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            var (user2TmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "From Another User",
                owner: "Owner2",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered, 
                DataProvider.USPTO, 
                classNumbers: new[] { 18 });

            testDbContext.TrademarkRegistrations.AddRange(user1TmEntity1, user1TmEntity2, user2TmEntity);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity1.Id);
            await userTmRepository.AddOrUndeleteAsync(user1.Id, user1TmEntity2.Id);
            await userTmRepository.SoftRemoveAsync(user1.Id, user1TmEntity2.Id);

            await userTmRepository.AddOrUndeleteAsync(user2.Id, user2TmEntity.Id);

            var queryLinksResult = userTmRepository.QueryUserLinks(user1.Id).ToList();

            // Asserts: only a single active link ("The Existing One") should be returned
            queryLinksResult.Should().HaveCount(1);

            var singleLink = queryLinksResult.Single();

            // Asserts that only "The Existing One" remains for user1, "The Removed One" was soft-deleted, "From Another User" belongs to user2
            singleLink.TrademarkRegistration!.Wordmark.Should().Be("The Existing One");

            // Asserts that the query result includes the correct trademark classes
            singleLink.TrademarkRegistration!.Classes!.Select(c => c.ClassNumber)
                .Should().BeEquivalentTo(new[] { 9, 25 });

            // Verifies that the entity is detached from the DbContext, ensuring AsNoTracking behavior
            testDbContext.Entry(singleLink).State.Should().Be(EntityState.Detached);

            // Verifies that the included entity is also detached from the DbContext
            testDbContext.Entry(singleLink.TrademarkRegistration!).State.Should().Be(EntityState.Detached);
        }
    }
}
