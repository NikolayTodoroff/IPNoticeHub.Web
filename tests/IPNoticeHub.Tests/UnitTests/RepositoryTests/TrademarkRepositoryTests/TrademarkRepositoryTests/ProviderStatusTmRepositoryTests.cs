using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class ProviderStatusTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_FilterByProviderOnly()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO);

            var (entity3, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending, 
                source: DataProvider.WIPO);

            testDbContext.TrademarkRegistrations.AddRange(
                entity1, 
                entity2, 
                entity3);

            testDbContext.SaveChanges();

            var repository = 
                new TrademarkRepository(testDbContext);

            var usptoOnly = 
                repository.Query(new TrademarkSearchFilter
            {
                Provider = DataProvider.USPTO
            }).
            Select(t => t.Wordmark).
            ToArray();

            usptoOnly.Should().BeEquivalentTo(new[] { "ALPHA" });
        }

        [Test]
        public void QueryRepository_FiltersByStatusOnly()
        {
            var (entity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (entity2, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO);

            var (entity3, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA",
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.WIPO);

            testDbContext.TrademarkRegistrations.AddRange(
                entity1, 
                entity2, 
                entity3);

            testDbContext.SaveChanges();

            var pendingOnly = repository.Query(
                new TrademarkSearchFilter
            {
                Status = TrademarkStatusCategory.Pending
            }).
            Select(t => t.Wordmark).
            ToArray();

            pendingOnly.Should().BeEquivalentTo(new[] { "BETA" });
        }

        [Test]
        public void QueryRepository_FilterBy_Provider_Status_Class_UseAndLogic()
        {
            var (matchingEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (wrongProviderEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA", 
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            var (wrongStatusEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (wrongClassEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA",
                owner: "user4",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4444444",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                matchingEntity, 
                wrongProviderEntity, 
                wrongStatusEntity, 
                wrongClassEntity);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            }).
            Select(t => t.Wordmark).
            ToArray();

            result.Should().Equal("ALPHA");
        }
    }
}
