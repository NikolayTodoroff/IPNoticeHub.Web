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
            using IPNoticeHubDbContext? testDbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            TrademarkRepository? repository = new TrademarkRepository(testDbContext);

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
            using IPNoticeHubDbContext? testDbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            TrademarkRepository? repository = new TrademarkRepository(testDbContext);

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
            using IPNoticeHubDbContext? testDbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            TrademarkRepository? trademarkRepository = new TrademarkRepository(testDbContext);

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
            using IPNoticeHubDbContext? testDbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            TrademarkRepository? trademarkRepository = new TrademarkRepository(testDbContext);

            IQueryable<TrademarkEntity>? partialOwnerMatches = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Black",
                ExactMatch = false
            });

            partialOwnerMatches.Should().ContainSingle().Which.Owner.Should().Be("Black Company LLC");
        }

        [Test]
        public void QueryRepository_FilterByRegistrationNumber_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = TestDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "A",
                owner: "OwnerA",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = TestDbContextFactory.CreateTrademark(
                wordmark: "B",
                owner: "OwnerB",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);


        }


    }
}
