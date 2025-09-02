using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;
using System.Linq;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks
{
    [TestFixture]
    public class TrademarkRepositoryTests
    {
        [Test]
        public void QueryRepository_FilterByWordmark_HandlesContainsAndExactMatchCorrectly()
        {
            // Arrange: Set up the in-memory database and seed it with test data
            using IPNoticeHubDbContext? dbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            dbContext.TrademarkRegistrations.AddRange(firstTrademark, secondTrademark);
            dbContext.SaveChanges();

            TrademarkRepository? repository = new TrademarkRepository(dbContext);

            // Act: Perform the query operations with the specified filters
            var exactMatchResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "Second Tower",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            var containsResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "FIRST",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            // Assert: Verify that the results match the expected output
            exactMatchResult.Should().Equal("Second Tower");
            containsResult.Should().Equal("FIRSTWAVE");
        }
    }
}
