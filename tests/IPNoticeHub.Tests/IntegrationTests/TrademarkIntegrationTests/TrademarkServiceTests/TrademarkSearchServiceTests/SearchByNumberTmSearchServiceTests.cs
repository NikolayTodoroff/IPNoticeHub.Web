using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using NUnit.Framework;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.TrademarkSearchServiceTests
{
    public class SearchByNumberTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesRegistrationNumber()
        {
            const string expectedSearchTerm = "1234567";

            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "Test Wordmark",
                owner: "Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: expectedSearchTerm,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var randomEntity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Random Test Wordmark",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices2",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "3322115",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = expectedSearchTerm,
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = 
                pagedResult.Results.Single();

            result.Id.Should().Be(entity.Id);
            result.Wordmark.Should().Be(entity.Wordmark);
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchTrue_MatchesSourceId()
        {
            const string expectedSearchTerm = "X123AZ";

            var entity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark",
               owner: "Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: expectedSearchTerm,
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO);

            var randomEntity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Random Test Wordmark",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = expectedSearchTerm,
                ExactMatch = true
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = pagedResult.Results.Single();

            result.Id.Should().Be(entity.Id);
            result.Wordmark.Should().Be(entity.Wordmark);
        }

        [Test]
        public async Task SearchAsync_WhenNumberExactMatchFalse_AllowsPartialMatches()
        {
            const string expectedSearchTerm = "123";

            var entity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark",
               owner: "Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "XWY125",
               statusDetail: "Successfully Registered",
               regNumber: $"789{expectedSearchTerm}",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO);

            var randomEntity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Random Test Wordmark",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "WWW654",
               statusDetail: "Awaiting Approval",
               regNumber: "9785670",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkFilterDto
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = expectedSearchTerm,
                ExactMatch = false
            };

            var pagedResult = 
                await service.SearchAsync(
                dto: dto,
                currentPage: 1,
                resultsPerPage: 10,
                cancellationToken: default);

            pagedResult.ResultsCount.Should().Be(1);
            pagedResult.Results.Should().ContainSingle();

            var result = pagedResult.Results.Single();

            result.Id.Should().Be(entity.Id);
            result.Wordmark.Should().Be(entity.Wordmark);
        }
    }
}
