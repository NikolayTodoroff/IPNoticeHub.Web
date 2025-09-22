using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Services.Application.DTOs;
using IPNoticeHub.Services.Application.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
using IPNoticeHub.Tests.UnitTests.TestUtilities;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class TmSearchQueryPagingTests
    {
        [Test]
        public async Task SearchAsync_Paging_ReturnsSecondItemOnPage2_AndKeepsTotal()
        {
            using IPNoticeHubDbContext testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (anubisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Anubis",
                owner: "Underworld Inc.",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDTO
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 2,
                PageSize = 1
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
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
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDTO
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = 3, //Out of range
                PageSize = 2
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
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
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (horusTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Horus",
                owner: "Falcon LLC",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (osirisTm, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Osiris",
                owner: "Afterlife Inc.",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.AddRange(anubisTm, horusTm, osirisTm);
            await testDbContext.SaveChangesAsync();

            ITrademarkReadRepository readRepo = new TestReadRepository(testDbContext);
            var queryService = new TrademarkSearchQueryService(readRepo);

            var query = new TrademarkSearchQueryDTO
            {
                Query = null, // Empty query => no text filter
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = await queryService.SearchAsync(query, CancellationToken.None);
            total.Should().Be(3);
            queryResult.Should().HaveCount(3);

            queryResult.Select(q => q.RegistrationNumber).Should().ContainInOrder("1111111", "2222222", "3333333");
        }
    }
}
