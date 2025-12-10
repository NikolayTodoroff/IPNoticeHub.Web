using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoProviderStatusTests
    {
        /// <summary>
        /// Section: Provider / Status filters & AND-composition
        /// Ensures that standalone Provider and Status filters work independently,
        /// and when combined with ClassNumbers they compose with AND semantics:
        /// a row must satisfy ALL provided filters to be returned.
        /// </summary>
        [Test]
        public void QueryRepository_FilterByProviderOnly()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (A1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (B2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO);

            var (C3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending, 
                source: DataProvider.WIPO);

            testDbContext.TrademarkRegistrations.AddRange(A1, B2, C3);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var usptoOnly = trademarkRepository.Query(new TrademarkSearchFilter
            {
                Provider = DataProvider.USPTO
            }).Select(t => t.Wordmark).ToArray();

            usptoOnly.Should().BeEquivalentTo(new[] { "ALPHA" });
        }

        [Test]
        public void QueryRepositoru_FiltersByStatusOnly()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (A1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (B2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.EUIPO);

            var (C3, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA",
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Abandoned,
                source: DataProvider.WIPO);

            testDbContext.TrademarkRegistrations.AddRange(A1, B2, C3);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var pendingOnly = trademarkRepository.Query(new TrademarkSearchFilter
            {
                Status = TrademarkStatusCategory.Pending
            }).Select(t => t.Wordmark).ToArray();

            pendingOnly.Should().BeEquivalentTo(new[] { "BETA" });
        }

        [Test]
        public void QueryRepository_FilterBy_Provider_Status_Class_UseAndLogic()
        {
            using var ctx = InMemoryDbContextFactory.CreateTestDbContext();

            // Matches ALL filters: Provider=USPTO, Status=Registered, Class=25
            var (matchingEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            // Distractor 1: wrong provider (EUIPO)
            var (wrongProviderEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA", 
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            // Distractor 2: wrong status (Pending)
            var (wrongStatusEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA", 
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            // Distractor 3: missing class 25 (has 30 only)
            var (wrongClassEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA",
                owner: "user4",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4444444",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            ctx.TrademarkRegistrations.AddRange(matchingEntity, wrongProviderEntity, wrongStatusEntity, wrongClassEntity);
            ctx.SaveChanges();

            var trademarkRepository = new TrademarkRepository(ctx);

            var result = trademarkRepository.Query(new TrademarkSearchFilter
            {
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            }).Select(t => t.Wordmark).ToArray();

            result.Should().Equal("ALPHA");
        }
    }
}
