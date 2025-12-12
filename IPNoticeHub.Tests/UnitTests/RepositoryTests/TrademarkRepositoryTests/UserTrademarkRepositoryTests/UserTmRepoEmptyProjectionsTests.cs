using FluentAssertions;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.UserTrademarkRepositoryTests
{
    [TestFixture]
    public class UserTmRepoEmptyProjectionsTests
    {
        [Test]
        public async Task GetUserCollection_Empty_ReturnsEmpty_NoExceptions()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = InMemoryDbContextFactory.CreateApplicationUser(id: "user1");
            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            var userTmRepository = new UserTrademarkRepository(testDbContext);

            var pageResult = await userTmRepository.GetUserCollectionPageAsync(
                user.Id,
                CollectionSortBy.DateAddedDesc,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pageResult.Results.Should().BeEmpty();
            pageResult.ResultsCount.Should().Be(0);
            pageResult.CurrentPage.Should().Be(1);
            pageResult.ResultsCountPerPage.Should().Be(10);
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
