using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoClassFiltersTests
    {
        [Test]
        public void QueryRepository_FilterBySingleClass_ReturnsTrademarksForThatClass()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AZ1",
                owner: "Barry Douglas",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "XY2",
                owner: "Leonard Cohen",
                goodsAndServices: "testGoodsAndSerices02",
                sourceId: "testSourceId02",
                statusDetail: "testStatusDetail02",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            var (thirdTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "NM3",
                owner: "Denis Rodman",
                goodsAndServices: "testGoodsAndSerices03",
                sourceId: "testSourceId03",
                statusDetail: "testStatusDetail03",
                regNumber: "7162365",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9,25 });

            testDbContext.TrademarkRegistrations.AddRange(
                firstTestTrademark, 
                secondTestTrademark,
                thirdTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var queryResult = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25 }
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().
                BeEquivalentTo(new[] { "AZ1", "NM3" });
        }

        [Test]
        public void QueryRepository_FilterByMultipleClasses_UsesOrLogic()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "A1",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "B2",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices02",
                sourceId: "testSourceId02",
                statusDetail: "testStatusDetail02",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            var (thirdTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "C3",
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices03",
                sourceId: "testSourceId03",
                statusDetail: "testStatusDetail03",
                regNumber: "1177224",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9 });

            var (fourthTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "D4",
                owner: "user4",
                goodsAndServices: "testGoodsAndSerices04",
                sourceId: "testSourceId04",
                statusDetail: "testStatusDetail04",
                regNumber: "7766552",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            testDbContext.TrademarkRegistrations.AddRange(
                firstTestTrademark, 
                secondTestTrademark, 
                thirdTestTrademark,
                fourthTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var result = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 9, 25 }
            }).
            Select(t => t.Wordmark).
            ToArray();

            result.Should().
                BeEquivalentTo(new[] { "A1", "C3", "D4" });
        }

        [Test]
        public void QueryRepository_FilterByClass_DuplicateValues_BehavesLikeDistinct()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (firstTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "A1",
                owner: "user1",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (secondTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "B2",
                owner: "user2",
                goodsAndServices: "testGoodsAndSerices02",
                sourceId: "testSourceId02",
                statusDetail: "testStatusDetail02",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 30 });

            var (thirdTestTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "C3",
                owner: "user3",
                goodsAndServices: "testGoodsAndSerices03",
                sourceId: "testSourceId03",
                statusDetail: "testStatusDetail03",
                regNumber: "1177224",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9 });

            testDbContext.TrademarkRegistrations.AddRange(
                firstTestTrademark, 
                secondTestTrademark, 
                thirdTestTrademark);

            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var duplicateFilterQuery = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25, 25 }
            }).
            Select(t => t.Wordmark).
            OrderBy(x => x).
            ToArray();

            var distinctFilterQuery = trademarkRepository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25 }
            }).
            Select(t => t.Wordmark).
            OrderBy(x => x).
            ToArray();

            duplicateFilterQuery.Should().
                Equal(distinctFilterQuery).
                And.BeEquivalentTo(new[] { "A1", "B2" });
        }
    }
}
