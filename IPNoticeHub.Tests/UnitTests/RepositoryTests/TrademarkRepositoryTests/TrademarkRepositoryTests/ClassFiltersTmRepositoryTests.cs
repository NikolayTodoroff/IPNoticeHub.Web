using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class ClassFiltersTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_FilterBySingleClass_ReturnsTrademarksForThatClass()
        {
            var (entity1, _) = 
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

            var (entity2, _) = 
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

            var (entity3, _) = 
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
                entity1, 
                entity2,
                entity3);

            testDbContext.SaveChanges();

            var queryResult = repository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25 }
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().BeEquivalentTo(new[] { "AZ1", "NM3" });
        }

        [Test]
        public void QueryRepository_FilterByMultipleClasses_UsesOrLogic()
        {
            var (entity1, _) = 
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

            var (entity2, _) = 
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

            var (entity3, _) = 
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

            var (entity4, _) = 
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
                entity1, 
                entity2, 
                entity3,
                entity4);

            testDbContext.SaveChanges();

            var result = repository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 9, 25 }
            }).
            Select(t => t.Wordmark).
            ToArray();

            result.Should().BeEquivalentTo(new[] { "A1", "C3", "D4" });
        }

        [Test]
        public void QueryRepository_FilterByClass_DuplicateValues_BehavesLikeDistinct()
        {
            var (entity1, _) = 
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

            var (entity2, _) = 
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

            var (entity3, _) = 
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
                entity1, 
                entity2, 
                entity3);

            testDbContext.SaveChanges();

            var duplicateFilterQuery = repository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25, 25 }
            }).
            Select(t => t.Wordmark).
            OrderBy(x => x).
            ToArray();

            var distinctFilterQuery = repository.Query(
                new TrademarkSearchFilter
            {
                ClassNumbers = new[] { 25 }
            }).
            Select(t => t.Wordmark).
            OrderBy(x => x).
            ToArray();

            duplicateFilterQuery.
                Should().Equal(distinctFilterQuery).
                And.BeEquivalentTo(new[] { "A1", "B2" });
        }
    }
}
