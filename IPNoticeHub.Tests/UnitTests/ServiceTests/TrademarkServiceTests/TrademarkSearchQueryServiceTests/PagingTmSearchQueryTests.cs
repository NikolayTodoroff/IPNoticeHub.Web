using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class PagingTmSearchQueryTests
    {
        [Test]
        public async Task SearchAsync_Paging_ReturnsSecondItemOnPage2_AndKeepsTotal()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository repository = new TestReadRepository(testDbContext);
            var service = new TrademarkSearchQueryService(repository);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 2,
                PageSize = 1
            };

            var (queryResult, total) = await service.SearchAsync(query, CancellationToken.None);
            total.Should().Be(3);
            queryResult.Single().RegistrationNumber.Should().Be("2222222");
        }

        [Test]
        public async Task SearchAsync_Paging_OutOfRange_ReturnsEmpty_AndKeepsTotal()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository repository = new TestReadRepository(testDbContext);
            var service = new TrademarkSearchQueryService(repository);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 3, //Out of range
                PageSize = 2
            };

            var (queryResult, total) = await service.SearchAsync(query, CancellationToken.None);
            total.Should().Be(3);
            queryResult.Should().BeEmpty();
        }

        [Test]
        public async Task SearchAsync_WithEmptyQueryAndNoFilters_ReturnsAll_OrderedByRegistrationNumber()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository repository = new TestReadRepository(testDbContext);
            var service = new TrademarkSearchQueryService(repository);

            var query = new TrademarkSearchQueryDto
            {
                Query = null, // Empty query => no text filter
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await service.SearchAsync(query, CancellationToken.None);
            total.Should().Be(3);
            queryResult.Should().HaveCount(3);

            queryResult.Select(q => q.RegistrationNumber).Should().ContainInOrder("1111111", "2222222", "3333333");
        }

        [Test]
        public async Task SearchAsync_WhenPageIsZero_TreatsAsPage1_ReturnsFirstSlice()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository repository = new TestReadRepository(testDbContext);
            var service = new TrademarkSearchQueryService(repository);

            var query = new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 0,
                PageSize = 1
            };

            var (queryResult, total) = await service.SearchAsync(query, CancellationToken.None);
            total.Should().Be(3);
            queryResult.Should().ContainSingle();

            queryResult.Single().RegistrationNumber.Should().Be("1111111");
        }
    }
}
