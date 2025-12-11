using FluentAssertions;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    [TestFixture]
    public class UserTmRepoEmptyProjectionsTests
    {
        [Test]
        public void QueryUserCollection_Empty_ReturnsEmpty_NoExceptions()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);
            testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var queryLinksResult = 
                userTmRepository.QueryUserCollection(user.Id).
                ToArray();

            queryLinksResult.Should().
                BeEmpty();
        }

        [Test]
        public void QueryUserLinks_Empty_ReturnsEmpty_NoExceptions()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var user = 
                InMemoryDbContextFactory.CreateApplicationUser("user1");

            testDbContext.Users.Add(user);
            testDbContext.SaveChangesAsync();

            var userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var queryLinksResult = 
                userTmRepository.QueryUserLinks(user.Id).
                ToArray();

            queryLinksResult.Should().
                BeEmpty();
        }
    }
}
