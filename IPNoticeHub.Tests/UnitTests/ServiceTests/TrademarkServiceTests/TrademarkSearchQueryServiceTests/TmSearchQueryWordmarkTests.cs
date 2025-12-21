using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    [TestFixture]
    public class TmSearchQueryWordmarkTests
    {
        [Test]
        public async Task SearchAsync_WhenWordmarkContainsQuery_ReturnsMatchingTrademark()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Diablo Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "54321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmZZZ, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "DevilHunter LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "11111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(tmAAA, tmZZZ);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "first",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult,total) = await queryService.SearchAsync(query,CancellationToken.None);
            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be("First WM");
            queryItem.Owner.Should().Be("Diablo Inc.");
            queryItem.RegistrationNumber.Should().Be("54321");
            queryItem.Status.Should().Be(TrademarkStatusCategory.Registered.ToString());
            queryItem.Id.Should().Be(tmAAA.Id);
        }

        [Test]
        public async Task SearchAsync_WhenWordmarkIdenticalQuery_ReturnsMatchingTrademark()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmAAA, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Diablo Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "54321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmZZZ, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "DevilHunter LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "11111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(tmAAA, tmZZZ);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "First WM",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be("First WM");
            queryItem.Owner.Should().Be("Diablo Inc.");
            queryItem.RegistrationNumber.Should().Be("54321");
            queryItem.Status.Should().Be(TrademarkStatusCategory.Registered.ToString());
            queryItem.Id.Should().Be(tmAAA.Id);
        }

        [Test]
        public async Task SearchAsync_WhenOwnerContainsQuery_ReturnsMatchingOwners()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm,horusTm,osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "Inc",
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(2);
            queryResult.Select(i => i.Owner).Should().BeEquivalentTo(new[] { "Underworld Inc.", "Afterlife Inc." });
        }

        [Test]
        public async Task SearchAsync_WhenOwnerIdentityQuery_ReturnsOnlyMatchingOwners()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "Falcon LLC",
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(1);
            queryResult.Single().Owner.Should().Be("Falcon LLC");
        }

        [Test]
        public async Task SearchAsync_WhenNumberIdenticalQuery_MatchesOnlyExactRegistrationNumber()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "1234567",
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(1);
            queryResult.Single().RegistrationNumber.Should().Be("1234567");
        }

        [Test]
        public async Task SearchAsync_WhenNumberContainsQuery_MatchesOnlyExactRegistrationNumber()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234589",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "12345",
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(2);
            queryResult.Select(q => q.RegistrationNumber).Should().BeEquivalentTo(new[] { "1234567", "1234589" });
        }

        [Test]
        public async Task SearchAsync_WithStatusRegistered_FiltersOnlyRegistered()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Cancelled,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Status = TrademarkStatusCategory.Registered,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(1);
            queryResult.Single().Wordmark.Should().Be("Anubis");
        }

        [Test]
        public async Task SearchAsync_WithClass25_FiltersOnlyItemsHavingClass25()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25,35 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1122334",
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3355442",
                status: TrademarkStatusCategory.Cancelled,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Class = (TrademarkClass)25,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(2);
            queryResult.Select(q => q.Wordmark).Should().BeEquivalentTo(new[] { "Anubis", "Osiris" });
        }
    }
}
