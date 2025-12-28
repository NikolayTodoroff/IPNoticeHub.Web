using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class FilterByStatusTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_FilterByProvider_Class_Status_And_Includes_Nav()
        {
            var (matchingTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1111111",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            var (wrongProviderTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2222222",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.EUIPO,
                classNumbers: new[] { 25 });

            var (wrongStatusTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "GAMMA",
                owner: "OwnerC",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "3333333",
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (wrongClassTrademark, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA",
                owner: "OwnerD",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "4444444",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.AddRange(
                matchingTrademark, 
                wrongProviderTrademark, 
                wrongStatusTrademark, 
                wrongClassTrademark);

            testDbContext.SaveChanges();

            TrademarkEntity[]? queryResult = 
                repository.Query(new TrademarkSearchFilter()
            {
                Provider = DataProvider.USPTO,
                Status = TrademarkStatusCategory.Registered,
                ClassNumbers = new[] { 25 }
            }, includeNav: true).
            ToArray();

            queryResult.Select(r => r.Wordmark).
                Should().Equal("ALPHA");

            queryResult.Single().Classes.Should().NotBeNull();

            queryResult.Single().Classes.
                Should().Contain(c => c.ClassNumber == 25);
        }
    }
}
