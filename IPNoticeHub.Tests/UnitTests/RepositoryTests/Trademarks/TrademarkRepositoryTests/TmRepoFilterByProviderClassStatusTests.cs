using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoFilterByProviderClassStatusTests
    {
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
    }
}
