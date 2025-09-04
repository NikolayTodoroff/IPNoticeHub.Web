using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks
{
    [TestFixture]
    public class TmRepoInputSanityTests
    {
        /// <summary>
        /// Section: Sanity Checks and Default Behavior Validation
        /// Validate that the repository behaves correctly with minimal or no filtering applied.
        /// These tests ensure that default behavior does not unintentionally over-filter results.
        /// Assertions include:
        /// - Default filter retrieves all records.
        /// - Whitespace-only SearchTerm is ignored.
        /// - Null or empty ClassNumbers applies no class filter.
        /// </summary>
        [Test]
        public void QueryRepository_WithDefaultFilter_ReturnsAllTrademarks()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA1",
                owner: "OwnerA",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA2",
                owner: "OwnerB",
                regNumber: "7654321",
                TrademarkStatusCategory.Pending,
                DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter()).Select(t => t.Wordmark).ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "ALPHA1", "BETA2" });
        }

        [Test]
        public void QueryRepository_Ignores_Whitespace_SearchTerm()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var whitespaceQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "   ",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            whitespaceQueryResult.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }

        [Test]
        public void QueryRepository_ClassNumbers_NullOrEmpty_ReturnsAllTrademarks()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var nullClassQueryResult = trademarkRepository.Query(new TrademarkSearchFilter { ClassNumbers = null }).
                                 Select(t => t.Wordmark).
                                 ToArray();

            nullClassQueryResult.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });


            var emptyClassQueryResult = trademarkRepository.Query(new TrademarkSearchFilter { ClassNumbers = new int[0] }).
                                 Select(t => t.Wordmark).
                                 ToArray();

            emptyClassQueryResult.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }
    }
}
