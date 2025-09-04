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
        /// <summary>
        /// Section: Wordmark Search Semantics
        /// Ensures the repository correctly applies the Wordmark filter in both Exact and Contains modes.
        /// Validates case-insensitive and null-safe matching using clean, unambiguous data.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByWordmark_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var repository = new TrademarkRepository(testDbContext);

            string[]? wordmarkExactMatchResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "Second Tower",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            wordmarkExactMatchResult.Should().Equal("Second Tower");
        }

        [Test]
        public void QueryRepository_FilterByWordmark_ReturnsResults_ForPartialMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "FIRSTWAVE",
                owner: "Alan Smith",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second Tower",
                owner: "Buddha Park Ltd",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var repository = new TrademarkRepository(testDbContext);

            string[]? wordmarkPartialMatchResult = repository.Query(new TrademarkSearchFilter()
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "FIRST",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            wordmarkPartialMatchResult.Should().Equal("FIRSTWAVE");
        }

        /// <summary>
        /// Section: Owner Search Semantics
        /// Validates that the repository correctly filters trademarks by Owner in both Exact and Partial match modes.
        /// Ensures case-insensitive matching and proper handling of null or ambiguous data.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByOwner_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

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
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dark Moon",
                owner: "Black Company LLC",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Seven Days",
                owner: "White Trades Inc",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            IQueryable<TrademarkEntity>? partialOwnerMatches = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Black",
                ExactMatch = false
            });

            partialOwnerMatches.Should().ContainSingle().Which.Owner.Should().Be("Black Company LLC");
        }

        /// <summary>
        /// Section: Number Search (RegistrationNumber or SourceId)
        /// Ensures that "Number" searches correctly match either the RegistrationNumber or SourceId/Serial.
        /// Tests validate both Exact and Partial match scenarios with typical inputs, leveraging the repository's normalization logic.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByNumber_RegistrationNumber_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (registryNumberTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "A", 
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (serialNumberTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "OMEGA", 
                owner: "B", 
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            testDbContext.TrademarkRegistrations.AddRange(registryNumberTrademark, serialNumberTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "us-111-abc",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            result.Should().Equal("ALPHA");
        }

        [Test]
        public void QueryRepository_FilterByNumber_RegistrationNumber_ReturnsResults_ForPartialMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (registryNumberTrademark1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "Alex T", 
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            var (registryNumberTrademark2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Beta",
                owner: "George Orwell",
                regNumber: "US-201-aBc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (randomTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "George B", 
                regNumber: "US-999-ZZZ",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(registryNumberTrademark1, registryNumberTrademark2,randomTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",   
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            result.Should().BeEquivalentTo(new[] { "ALPHA", "Beta" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_SerialNumber_SourceId_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (serialNumberTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dead End", 
                owner: "Elton John", 
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned, 
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            var (registryNumberTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BB12", 
                owner: "Osho", 
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(serialNumberTrademark, registryNumberTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "sn-222-xyz",   
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            result.Should().Equal("Dead End");
        }

        [Test]
        public void QueryRepository_FilterByNumber_SerialNumber_SourceId_ReturnsResults_ForPartialMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (serialNumberTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "S&P500", 
                owner: "B", 
                regNumber: null,
                status: TrademarkStatusCategory.Abandoned, 
                source: DataProvider.USPTO);
                serialNumberTrademark.SourceId = "SN 222-xyz";

            var (randomTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Delta Force", 
                owner: "D", 
                regNumber: "US-333-QQQ",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(serialNumberTrademark, randomTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "222",   
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            result.Should().BeEquivalentTo(new[] { "S&P500" });
        }

        /// <summary>
        /// Section: Provider, Class, and Status filtering with navigation properties
        /// Validates that the repository correctly filters trademarks based on the specified provider, status, and class numbers.
        /// Ensures that navigation properties are included when requested.
        /// Verifies that only trademarks matching all criteria are returned, and navigation properties are populated.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByProvider_Class_Status_And_Includes_Nav()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (matchingTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "OwnerA", 
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (wrongProviderTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA", 
                owner: "OwnerB", 
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            var (wrongStatusTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "OwnerC", 
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending, 
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (wrongClassTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA", 
                owner: "OwnerD", 
                regNumber: "4444444",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(matchingTrademark, wrongProviderTrademark, wrongStatusTrademark, wrongClassTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            TrademarkEntity[]? queryResult = trademarkRepository.Query(new TrademarkSearchFilter()
            {
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            }, includeNav: true).ToArray();

            queryResult.Select(r => r.Wordmark).Should().Equal("ALPHA");

            queryResult.Single().Classes.Should().NotBeNull();
            queryResult.Single().Classes.Should().Contain(c => c.ClassNumber == 25);
        }

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
                wordmark:"ALPHA1",
                owner:"OwnerA",
                regNumber:"1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark:"BETA2",
                owner:"OwnerB",
                regNumber:"7654321",
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
                wordmark:"BETA",
                owner:"OwnerB",
                regNumber:"7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(firstTestTrademark, secondTestTrademark);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var whitespaceQuery = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "   ",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            whitespaceQuery.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
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

            var nullClassQuery = trademarkRepository.Query(new TrademarkSearchFilter { ClassNumbers = null }).
                                 Select(t => t.Wordmark).
                                 ToArray();

            nullClassQuery.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });


            var emptyClassQuery = trademarkRepository.Query(new TrademarkSearchFilter { ClassNumbers = new int[0] }).
                                 Select(t => t.Wordmark).
                                 ToArray();

            emptyClassQuery.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }
    }
}
