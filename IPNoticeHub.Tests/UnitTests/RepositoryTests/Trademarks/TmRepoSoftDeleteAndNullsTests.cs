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
    public class TmRepoSoftDeleteAndNullsTests
    {
        /// <summary>
        /// Section: Field resilience with empty and non-nullable fields
        /// Tests the repository's behavior when querying trademarks with empty or non-nullable fields.
        /// Verifies that empty string fields do not produce matches for partial or exact filters 
        /// and ensures stability when no results are found.
        /// </summary>
        [Test]
        public void QueryRepository_EmptyWordmarkInput_DoesNotMatch_OnPartialWordmarkFilters()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (emptyEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: string.Empty,
                owner: "Alladin T",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Manson D",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(emptyEntity, validEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var partialQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "tar",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            partialQueryResult.Should().BeEquivalentTo(new[] { "Target" });
        }

        [Test]
        public void QueryRepository_EmptyWordmarkInput_DoesNotMatch_OnExactWordmarkFilters()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (emptyEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: string.Empty,
                owner: "Alladin T",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Manson D",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(emptyEntity, validEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var exactQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "target",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            exactQueryResult.Should().Equal("Target");
        }

        [Test]
        public void QueryRepository_EmptyOwner_DoesNotMatch_OnExactOwnerFilters()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (emptyOwnerEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Neverending Ending",
                owner: string.Empty,
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Marlon Brando",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(emptyOwnerEntity, validEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var exactQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Marlon Brando",
                ExactMatch = true
            }).Select(t => t.Owner).ToArray();

            exactQueryResult.Should().Equal("Marlon Brando");
        }

        [Test]
        public void QueryRepository_EmptyOwner_DoesNotMatch_OnPartialOwnerFilters()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (emptyOwnerEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Neverending Ending",
                owner: string.Empty,
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Marlon Brando",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(emptyOwnerEntity, validEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var partialQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "brando",
                ExactMatch = false
            }).Select(t => t.Owner).ToArray();

            partialQueryResult.Should().BeEquivalentTo(new[] { "Marlon Brando" });
        }

        [Test]
        public void QueryRepository_EmptyResult_IsStable_NoExceptions()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "Owner", 
                regNumber: "123",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var emptyQuery = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "NO-MATCHING-SEARCH-TERM",
                ExactMatch = false
            }).ToArray();

            emptyQuery.Should().BeEmpty();
        }

    }
}
