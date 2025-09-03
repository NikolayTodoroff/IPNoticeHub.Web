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

        [Test]
        public void QueryRepository_FilterByRegistrationNumber_ReturnsResults_ForExactMatch()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "A",
                owner: "OwnerA",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (secondTestTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "B",
                owner: "OwnerB",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);
        }

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
                classNumbers: 25);

            var (wrongStatusTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "OwnerC", 
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending, 
                source: DataProvider.USPTO,
                classNumbers: 25);

            var (wrongClassTrademark, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA", 
                owner: "OwnerD", 
                regNumber: "4444444",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO,
                classNumbers: 30);

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
