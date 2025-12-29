using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class WordmarkTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_WhenWordmarkContainsQuery_ReturnsMatchingTrademark()
        {
            const string searchQueryFirstHalf = "Correct";
            const string searchQuerySecondHalf = "Entity";

            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: $"{searchQueryFirstHalf} {searchQuerySecondHalf}",
                owner: "Correct Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
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
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = searchQueryFirstHalf,
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be(entity.Wordmark);
            queryItem.Owner.Should().Be(entity.Owner);
            queryItem.RegistrationNumber.Should().Be(entity.RegistrationNumber);
            queryItem.Status.Should().Be(entity.StatusCategory.ToString());
            queryItem.Id.Should().Be(entity.Id);
        }

        [Test]
        public async Task SearchAsync_WhenWordmarkIdenticalQuery_ReturnsMatchingTrademark()
        {
            const string expectedSearchQuery = "Find Me!";

            var entity =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: expectedSearchQuery,
                owner: "Correct Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
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
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = expectedSearchQuery,
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be(entity.Wordmark);
            queryItem.Owner.Should().Be(entity.Owner);
            queryItem.RegistrationNumber.Should().Be(entity.RegistrationNumber);
            queryItem.Status.Should().Be(entity.StatusCategory.ToString());
            queryItem.Id.Should().Be(entity.Id);
        }

        [Test]
        public async Task SearchAsync_WhenOwnerContainsQuery_ReturnsMatchingOwners()
        {
            const string firstOwnerName = "X1Core";
            const string secondOwnerName = "Z2Corp";
            const string expectedSearchTerm = firstOwnerName;

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "First Wave",
                owner: $"{firstOwnerName} {expectedSearchTerm}",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Second Wave",
               owner: $"{secondOwnerName} {expectedSearchTerm}",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "1234568",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = expectedSearchTerm,
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(2);
            queryResult.Select(i => i.Owner).
                Should().BeEquivalentTo(new[] { entity1.Owner, entity2.Owner });
        }

        [Test]
        public async Task SearchAsync_WhenOwnerIdentityQuery_ReturnsOnlyMatchingOwners()
        {
            const string firstOwnerName = "X1Core";
            const string secondOwnerName = "Z2Corp";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "First Wave",
                owner: firstOwnerName,
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Second Wave",
               owner: secondOwnerName,
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "1234568",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = firstOwnerName,
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);
            queryResult.Single().Owner.Should().Be(entity1.Owner);
        }

        [Test]
        public async Task SearchAsync_WhenNumberIdenticalQuery_MatchesOnlyExactRegistrationNumber()
        {
            const string firstOwnerRegNum = "X123456";
            const string secondOwnerRegNum = "Z654321";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "First Wave",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: firstOwnerRegNum,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Second Wave",
               owner: "Owner B",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: secondOwnerRegNum,
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = firstOwnerRegNum,
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);

            queryResult.Single().RegistrationNumber.
                Should().Be(entity1.RegistrationNumber);
        }

        [Test]
        public async Task SearchAsync_WhenNumberContainsQuery_MatchesOnlyExactRegistrationNumber()
        {
            const string firstOwnerRegNum = "X123456";
            const string secondOwnerRegNum = "X123555";
            const string expectedSearchTerm = "X123";

            var entity1 =
                InMemoryDbContextFactory.CreateTrademarkEntity(
                wordmark: "First Wave",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: firstOwnerRegNum,
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var entity2 =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Second Wave",
               owner: "Owner B",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: secondOwnerRegNum,
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = expectedSearchTerm,
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(2);

            var registrationNumbers = 
                queryResult.Select(q => q.RegistrationNumber).ToArray();

            registrationNumbers.Should().BeEquivalentTo(new[] { 
                entity1.RegistrationNumber,
                entity2.RegistrationNumber });
        }

        [Test]
        public async Task SearchAsync_WithStatusRegistered_FiltersOnlyRegistered()
        {
            var entity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark A",
               owner: "Correct Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "X123AZ",
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO);

            var randomEntity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Wordmark B",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO);

            testDbContext.TrademarkRegistrations.AddRange(entity, randomEntity);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = string.Empty,
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Status = TrademarkStatusCategory.Registered,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);
            queryResult.Single().Wordmark.Should().Be(entity.Wordmark);
        }

        [Test]
        public async Task SearchAsync_WithClass25_FiltersOnlyItemsHavingClass25()
        {
            var (entity1, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark A",
               owner: "Correct Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "X123AZ",
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO,
               classNumbers: new[] { 15, 35 });

            var (entity2, _) =
               InMemoryDbContextFactory.CreateTrademark(
               wordmark: "Wordmark B",
               owner: "Missing Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "D123AC",
               statusDetail: "Awaiting Approval",
               regNumber: "7654321",
               status: TrademarkStatusCategory.Pending,
               source: DataProvider.EUIPO,
               classNumbers: new[] { 10, 25 });

            testDbContext.TrademarkRegistrations.AddRange(entity1, entity2);
            await testDbContext.SaveChangesAsync();

            var dto = new TrademarkSearchQueryDto
            {
                Query = string.Empty,
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Class = (TrademarkClass)25,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);
            queryResult.Should().ContainSingle().
                Which.Wordmark.Should().Be(entity2.Wordmark);
        }
    }
}
