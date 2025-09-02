using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Tests.TestUtilities;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks
{
    [TestFixture]
    public class TrademarkRepositoryTests
    {
        [Test]
        public void Query_ByWordmark_ContainsVsExact_Works()
        {
            using var ctx = InMemoryDbFactory.Create();
            var (a, _) = InMemoryDbFactory.MakeTrademark(1, "ZENWAVE", "Zen Corp", "123", TrademarkStatusCategory.Registered, DataProvider.USPTO, 25);
            var (b, _) = InMemoryDbFactory.MakeTrademark(2, "ZEN", "Zen Labs", "456", TrademarkStatusCategory.Pending, DataProvider.USPTO, 30);
            ctx.TrademarkRegistrations.AddRange(a, b);
            ctx.SaveChanges();

            var repo = new TrademarkRepository(ctx);

            var contains = repo.Query(new TrademarkSearchFilter { SearchBy = TrademarkSearchBy.Wordmark, SearchTerm = "ZEN", ExactMatch = false })
                               .Select(t => t.Wordmark).ToArray();
            contains.Should().BeEquivalentTo(new[] { "ZENWAVE", "ZEN" });

            var exact = repo.Query(new TrademarkSearchFilter { SearchBy = TrademarkSearchBy.Wordmark, SearchTerm = "ZEN", ExactMatch = true })
                            .Select(t => t.Wordmark).ToArray();
            exact.Should().Equal(new[] { "ZEN" });
        }


    }
}
