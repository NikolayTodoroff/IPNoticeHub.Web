using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class IncludesNavTmRepositoryTests
    {
        [Test]
        public void QueryRepository_IncludeNav_False_DoesNotPopulateClasses()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var result = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "ALPHA",
                ExactMatch = true
            }, includeNav: false).Single();

            (result.Classes == null || result.Classes.Count == 0).Should().BeTrue();
        }

        [Test]
        public void QueryRepository_IncludeNav_True_PopulatesClasses()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTrademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (secondTrademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                firstTrademarkEntity, 
                secondTrademarkEntity);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                trademarkRepository.Query(new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25 }
            }, includeNav: true).
            ToArray();

            queryResult.Select(r => r.Wordmark).
                Should().Equal("ALPHA");

            queryResult.Single().Classes.Should().NotBeNull();

            queryResult.Single().Classes!.
                Select(c => c.ClassNumber).
                Should().BeEquivalentTo(new[] { 9, 25 });
        }
    }
}
