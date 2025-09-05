using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoNumberNormalizationTests
    {
        /// <summary>
        /// Section: Number normalization edges (RegistrationNumber OR SourceId/Serial)
        /// Validate that "Number" searches behave robustly against real-world formats:
        /// registration numbers and serial/application numbers often include spaces,
        /// dashes, slashes, dots, prefixes (e.g., "US-", "IR", "EUTM No"), or mixed case.
        /// The repository should normalize both sides so users can find marks regardless
        /// of formatting inconsistencies.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_RegistrationNumbers_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "us111abc",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_RegistrationNumbers_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                regNumber: "Us-111.ABc",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_Serial_SourceId_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Alpha & Omega",
                owner: "John Spencer",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            trademarkEntity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "ir123456789",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().Equal("Alpha & Omega");
        }

        [Test]
        public void QueryRepository_FilterByNumber_Normalizes_Serial_SourceId_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Bingo10",
                owner: "Michael Crafter",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            trademarkEntity.SourceId = "IR 123.456_789";

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "4567",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "Bingo10" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_NullSides_DoNotThrow_And_DoNotMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Dead and Divine",
                owner: "P Lower",
                regNumber: "US-999-XYZ",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (missingNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "XYZ 100",
                owner: "Frank Steward",
                regNumber: null,
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO);

            missingNumberEntity.SourceId = "SN-000-TEST";

            testDbContext.TrademarkRegistrations.AddRange(regNumberEntity, missingNumberEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "xyz",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "Dead and Divine" });
        }

        [Test]
        public void QueryRepository_FilterByNumber_NoMatch_ForExactSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AMZ",
                owner: "Liam Cooper",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(regNumberEntity, regNumberEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = true
            }).ToArray();

            queryResult.Should().BeEmpty();
        }

        [Test]
        public void QueryRepository_FilterByNumber_Match_ForPartialSearch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (regNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AMZ",
                owner: "Liam Cooper",
                regNumber: "US-111-ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(regNumberEntity, regNumberEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "abc",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "AMZ" });
        }
    }
}
