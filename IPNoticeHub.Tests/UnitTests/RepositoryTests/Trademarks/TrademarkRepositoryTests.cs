using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
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
            using var testdbContext = InMemoryDbFactory.CreateTestDbContext();

            var (entity1, _) = InMemoryDbFactory.CreateTrademark(wordmark: "ZENWAVE", owner: "Zen Corp", regNumber: "123", status: TrademarkStatusCategory.Registered, source: DataProvider.USPTO, classNumbers: new[] { 25 });
            var (entity2, _) = InMemoryDbFactory.CreateTrademark(wordmark: "ZEN", owner: "Zen Labs", regNumber: "456", status: TrademarkStatusCategory.Pending, source: DataProvider.USPTO, classNumbers: new[] { 30 });

            testdbContext.TrademarkRegistrations.AddRange(entity1, entity2);

            testdbContext.SaveChanges();

            TrademarkRepository? testTrademarkRepository = new TrademarkRepository(testdbContext);

            string[]? existingEntities = testTrademarkRepository.
                Query(new TrademarkSearchFilter { SearchBy = TrademarkSearchBy.Wordmark, SearchTerm = "ZEN", ExactMatch = false }).
                Select(t => t.Wordmark).
                ToArray();

            existingEntities.Should().BeEquivalentTo(new[] { "ZENWAVE", "ZEN" });

            string[]? exact = testTrademarkRepository.Query(new TrademarkSearchFilter { SearchBy = TrademarkSearchBy.Wordmark, SearchTerm = "ZEN", ExactMatch = true })
                            .Select(t => t.Wordmark).ToArray();

            exact.Should().Equal(new[] { "ZEN" });
        }
    }
}
