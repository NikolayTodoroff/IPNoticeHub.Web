using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;
using System.Text;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    [TestFixture]
    public class TmRepoInvalidAndEdgeInputsTests
    {
        [Test]
        public void QueryRepository_VeryLong_SearchTerm_DoesNotThrow_AndReturnsEmpty()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = InMemoryDbContextFactory.CreateTrademark(
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

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < 5000; i++)
            {
                stringBuilder.Append((char)('A' + i % 26));
            }

            var longSearchTerm = stringBuilder.ToString();

            var queryResults = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = longSearchTerm,
                ExactMatch = false
            }).ToArray();

            queryResults.Should().BeEmpty();
        }

        [Test]
        public void QueryRepository_FilterByNumber_SymbolHeavyInput_NormalizesAndMatches()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();
     
            var (regNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ALPHA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "US111ABC",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (serialNumberEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: null,
                status: TrademarkStatusCategory.Pending,
                source: DataProvider.USPTO);
                serialNumberEntity.SourceId = "SN-222-XYZ";

            testDbContext.TrademarkRegistrations.AddRange(regNumberEntity, serialNumberEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            var regResults = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = "  us-111.a_b c  ",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            regResults.Should().Equal("ALPHA");

            var serialResults = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Number,
                SearchTerm = " sn / 222 . xyz ",
                ExactMatch = true
            }).Select(t => t.Wordmark).ToArray();

            serialResults.Should().Equal("BETA");
        }

        [Test]
        public void QueryRepository_Unicode_Contains_Matches_Wordmark_And_Owner()
        {
            using IPNoticeHubDbContext? testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            // Accented characters in both fields
            var (unicodeCharsEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Café Zén",
                owner: "Niño Brands",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1001",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            var (standardCharsEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Plain Coffee",
                owner: "Other Co",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1002",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(unicodeCharsEntity, standardCharsEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = new TrademarkRepository(testDbContext);

            // Wordmark contains (case-insensitive, Unicode preserved)
            var wordmarkQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "zÉn",
                ExactMatch = false
            }).Select(t => t.Wordmark).ToArray();

            wordmarkQueryResult.Should().BeEquivalentTo(new[] { "Café Zén" });

            // Owner contains (case-insensitive, Unicode preserved)
            var ownerQueryResult = trademarkRepository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Owner,
                SearchTerm = "niÑO",
                ExactMatch = false
            }).Select(t => t.Owner).ToArray();

            ownerQueryResult.Should().BeEquivalentTo(new[] { "Niño Brands" });
        }
    }
}
