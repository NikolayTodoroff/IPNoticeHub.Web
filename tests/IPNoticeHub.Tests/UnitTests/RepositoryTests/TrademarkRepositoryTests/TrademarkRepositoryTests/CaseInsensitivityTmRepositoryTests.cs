using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class CaseInsensitivityTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void Query_Wordmark_ExactSearch_IsCaseInsensitive()
        {
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZENMARK",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "zEnmArk",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().ContainSingle("ZENMARK");
        }

        [Test]
        public void QueryRepository_FilterByWordmark_PartialSearch_IsCaseInsensitive()
        {
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZenithWave",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "wave",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            queryResult.Should().ContainSingle("ZenithWave");
        }

        [Test]
        public void QueryRepository_FilterByOwner_ExactSearch_IsCaseInsensitive()
        {
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "To The Moon And Back",
                owner: "Lunar Company LTD",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "lUnAr cOmPanY lTd",
                ExactMatch = true
            }).
            Select(t => t.Owner).
            ToArray();

            queryResult.Should().ContainSingle("Lunar Company LTD");
        }

        [Test]
        public void QueryRepository_FilterByOwner_PartialSearch_IsCaseInsensitive()
        {
            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "SolarWaves",
                owner: "Solar Tech Inc",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var results = 
                repository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "tech",
                ExactMatch = false
            }).
            Select(t => t.Owner).
            ToArray();

            results.Should().ContainSingle("Solar Tech Inc");
        }
    }
}
