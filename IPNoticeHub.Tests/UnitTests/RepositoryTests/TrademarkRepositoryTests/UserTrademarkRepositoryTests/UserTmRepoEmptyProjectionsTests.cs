using FluentAssertions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    /// <summary>
    /// Section: Validates repository stability with no active user links.
    /// Verifies that QueryUserCollection(userId) returns an empty sequence without exceptions.
    /// Ensures that QueryUserLinks(userId) returns an empty sequence without exceptions.
    /// </summary>
    [TestFixture]
    public class UserTmRepoEmptyProjectionsTests
    {
        [Test]
        public void QueryUserCollection_Empty_ReturnsEmpty_NoExceptions()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);
        
            testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            var queryLinksResult = userTmRepository.QueryUserCollection(user.Id).ToArray();

            queryLinksResult.Should().BeEmpty();
        }

        [Test]
        public void QueryUserLinks_Empty_ReturnsEmpty_NoExceptions()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser("user1");
            testDbContext.Users.Add(user);

            testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            var queryLinksResult = userTmRepository.QueryUserLinks(user.Id).ToArray();

            queryLinksResult.Should().BeEmpty();
        }
    }
}
