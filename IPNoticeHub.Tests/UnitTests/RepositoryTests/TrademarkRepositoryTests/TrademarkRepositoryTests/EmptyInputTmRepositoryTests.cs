using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class EmptyInputTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_EmptyWordmarkInput_DoesNotMatch_OnPartialWordmarkFilters()
        {
            var (emptyEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: string.Empty,
                owner: "Alladin T",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Manson D",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                emptyEntity, 
                validEntity);
            
            testDbContext.SaveChanges();

            var partialQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "tar",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            partialQueryResult.Should().BeEquivalentTo(new[] { "Target" });
        }

        [Test]
        public void QueryRepository_EmptyWordmarkInput_DoesNotMatch_OnExactWordmarkFilters()
        {
            var (emptyEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: string.Empty,
                owner: "Alladin T",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Manson D",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                emptyEntity, 
                validEntity);
            
            testDbContext.SaveChanges();

            var exactQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "target",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            exactQueryResult.Should().Equal("Target");
        }

        [Test]
        public void QueryRepository_EmptyOwner_DoesNotMatch_OnExactOwnerFilters()
        {
            var (emptyOwnerEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Neverending Ending",
                owner: string.Empty,
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Marlon Brando",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                emptyOwnerEntity, 
                validEntity);
            
            testDbContext.SaveChanges();

            var exactQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "Marlon Brando",
                ExactMatch = true
            }).
            Select(t => t.Owner).
            ToArray();

            exactQueryResult.Should().Equal("Marlon Brando");
        }

        [Test]
        public void QueryRepository_EmptyOwner_DoesNotMatch_OnPartialOwnerFilters()
        {
            var (emptyOwnerEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Neverending Ending",
                owner: string.Empty,
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (validEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Target",
                owner: "Marlon Brando",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                emptyOwnerEntity, 
                validEntity);
            
            testDbContext.SaveChanges();

            var partialQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "brando",
                ExactMatch = false
            }).
            Select(t => t.Owner).
            ToArray();

            partialQueryResult.Should().BeEquivalentTo(new[] { "Marlon Brando" });
        }

        [Test]
        public void QueryRepository_EmptyResult_IsStable_NoExceptions()
        {
            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "123",
                status: TrademarkStatusCategory.Registered, 
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var emptyQuery = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "NO-MATCHING-SEARCH-TERM",
                ExactMatch = false
            }).
            ToArray();

            emptyQuery.Should().BeEmpty();
        }
    }
}
