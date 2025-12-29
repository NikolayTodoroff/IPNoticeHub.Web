using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using NUnit.Framework;
using System.Text;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class InvalidQueryInputTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_VeryLong_SearchTerm_DoesNotThrow_AndReturnsEmpty()
        {
            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA", 
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var sb = new StringBuilder();

            for (int i = 0; i < 5000; i++)
            {
                sb.Append((char)('A' + i % 26));
            }

            var longSearchTerm = sb.ToString();

            var entity = 
                repository.Query(
                    new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = longSearchTerm,
                ExactMatch = false
            }).
            ToArray();

            entity.Should().BeEmpty();
        }

        [Test]
        public void QueryRepository_FilterByNumber_SymbolHeavyInput_NormalizesAndMatches()
        {
            var (regNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US111ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (serialNumberEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO);
                serialNumberEntity.SourceId = "SN-222-XYZ";

            testDbContext.TrademarkRegistrations.AddRange(
                regNumberEntity, 
                serialNumberEntity);

            testDbContext.SaveChanges();

            var repository = 
                new TrademarkRepository(testDbContext);

            var regResults = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "  us-111.a_b c  ",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            regResults.Should().Equal("ALPHA");

            var serialResults = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = " sn / 222 . xyz ",
                ExactMatch = true
            }).
            Select(t => t.Wordmark).
            ToArray();

            serialResults.Should().Equal("BETA");
        }

        [Test]
        public void QueryRepository_Unicode_Contains_Matches_Wordmark_And_Owner()
        {
            var (unicodeCharsEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Café Zén",
                owner: "Niño Brands",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1001",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (standardCharsEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Plain Coffee",
                owner: "Other Co",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1002",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                unicodeCharsEntity, 
                standardCharsEntity);

            testDbContext.SaveChanges();

            var wordmarkQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "zÉn",
                ExactMatch = false
            }).
            Select(t => t.Wordmark).
            ToArray();

            wordmarkQueryResult.Should().BeEquivalentTo(new[] { "Café Zén" });

            var ownerQueryResult = repository.Query(
                new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "niÑO",
                ExactMatch = false
            }).
            Select(t => t.Owner).
            ToArray();

            ownerQueryResult.Should().BeEquivalentTo(new[] { "Niño Brands" });
        }

        [Test]
        public void QueryRepository_ClassNumbers_NullOrEmpty_ReturnsAllTrademarks()
        {
            var (entity1, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (entity2, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices0",
                sourceId: "testSourceId0",
                statusDetail: "testStatusDetail0",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 30 });

            testDbContext.TrademarkRegistrations.
                AddRange(entity1, entity2);

            testDbContext.SaveChanges();

            var repository =
                new TrademarkRepository(testDbContext);

            var nullClassQueryResult = repository.Query(
                new TrademarkSearchFilter { ClassNumbers = null }).
                Select(t => t.Wordmark).
                ToArray();

            nullClassQueryResult.
                Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });

            var emptyClassQueryResult = repository.Query(
                new TrademarkSearchFilter { ClassNumbers = new int[0] }).
                Select(t => t.Wordmark).
                ToArray();

            emptyClassQueryResult.
                Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }

        [Test]
        public void QueryRepository_Ignores_Whitespace_SearchTerm()
        {
            var (entity1, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "OwnerA",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            var (entity2, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "OwnerB",
                goodsAndServices: "testGoodsAndSerices0",
                sourceId: "testSourceId0",
                statusDetail: "testStatusDetail0",
                regNumber: "7654321",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(
                entity1,
                entity2);

            testDbContext.SaveChanges();

            var repository =
                new TrademarkRepository(testDbContext);

            var whitespaceQueryResult = repository.Query(
                new TrademarkSearchFilter
                {
                    SearchBy = TrademarkSearchBy.Wordmark,
                    SearchTerm = "   ",
                    ExactMatch = false
                }).
                Select(t => t.Wordmark).
                ToArray();

            whitespaceQueryResult.Should().BeEquivalentTo(new[] { "ALPHA", "BETA" });
        }
    }
}
