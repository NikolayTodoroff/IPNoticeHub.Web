using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Application.Repositories.TrademarkRepository;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    [TestFixture]
    public class TmSearchServiceSearchByNumberSemanticsTests
    {
        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesRegistrationNumber()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "1234567",
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            var singleTmSummaryDTO = 
                pagedResult.Results.Single();

            singleTmSummaryDTO.Id.Should().
                Be(tmEntity1.Id);

            singleTmSummaryDTO.Wordmark.Should().
                Be("First WM");
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesSourceId()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 9, 35 });
            tmEntity1.SourceId = "EU4546576454333";

            var (tmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "EU4546576454333",
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            var dto = pagedResult.Results.Single();

            dto.Id.Should().Be(tmEntity1.Id);

            dto.Wordmark.Should().
                Be("First WM");
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchFalse_AllowsPartialMatches()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "First WM",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            var (tmEntity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Second WM",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var filterDTO = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "123",
                ExactMatch = false
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: filterDTO,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().
                Be(1);

            pagedResult.Results.Should().
                ContainSingle();

            var dto = pagedResult.Results.Single();

            dto.Id.Should().
                Be(tmEntity1.Id);

            dto.Wordmark.Should().
                Be("First WM");
        }
    }
}
