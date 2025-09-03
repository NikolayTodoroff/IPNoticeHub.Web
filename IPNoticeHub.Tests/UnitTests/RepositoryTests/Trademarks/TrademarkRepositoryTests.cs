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
        public void QueryRepository_FilterByWordmark_HandlesExactMatchCorrectly()
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

            string[]? wordmarkExactMatchResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "Second Tower",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            wordmarkExactMatchResult.Should().Equal("Second Tower");
        }

        [Test]
        public void QueryRepository_FilterByWordmark_HandlesPartialMatchCorrectly()
        {
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

            string[]? wordmarkPartialMatchResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "FIRST",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            wordmarkPartialMatchResult.Should().Equal("FIRSTWAVE");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? dbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            dbContext.AddRange(firstTrademark, secondTrademark);
            dbContext.SaveChanges();

            TrademarkRepository? trademarkRepository = new TrademarkRepository(dbContext);

            IQueryable<TrademarkEntity>? exactOwnerMatches = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "White Trades Inc",
                ExactMatch = true
            });

            exactOwnerMatches.Should().ContainSingle().Which.Owner.Should().Be("White Trades Inc");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ReturnsResults_ForPartialMatch()
        {
            using IPNoticeHubDbContext? dbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            dbContext.AddRange(firstTrademark, secondTrademark);
            dbContext.SaveChanges();

            TrademarkRepository? trademarkRepository = new TrademarkRepository(dbContext);

            IQueryable<TrademarkEntity>? partialOwnerMatches = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Black",
                ExactMatch = false
            });

            partialOwnerMatches.Should().ContainSingle().Which.Owner.Should().Be("Black Company LLC");
        }

    }
}
