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
    public class TmRepoCaseInsensitivityContractTests
    {
        /// <summary>
        /// Section: Case-insensitivity contracts
        /// Ensure that Wordmark and Owner searches ignore case for both Exact and Contains modes.
        /// Trademark data is stored with inconsistent capitalization, and users rarely type search
        /// terms with perfect casing. If queries ever become case-sensitive, valid marks could be
        /// missed silently, causing broken search results in the UI.
        /// </summary>
        [Test]
        public void Query_Wordmark_ExactSearch_IsCaseInsensitive()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZENMARK",
                owner: "Owner",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "zEnmArk",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().ContainSingle("ZENMARK");
        }

        [Test]
        public void QueryRepository_FilterByWordmark_PartialSearch_IsCaseInsensitive()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZenithWave",
                owner: "Owner",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "wave",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            queryResult.Should().ContainSingle("ZenithWave");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ExactSearch_IsCaseInsensitive()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "To The Moon And Back",
                owner: "Lunar Company LTD",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "lUnAr cOmPanY lTd",
                ExactMatch = true
            }).Select(t => t.Owner).ToArray();

            queryResult.Should().ContainSingle("Lunar Company LTD");
        }

        [Test]
        public void QueryRepository_FilterByOwner_PartialSearch_IsCaseInsensitive()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "SolarWaves",
                owner: "Solar Tech Inc",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var results = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "tech",
                ExactMatch = false
            }).Select(t => t.Owner).ToArray();

            results.Should().ContainSingle("Solar Tech Inc");
        }

    }
}
