using FluentAssertions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class IncludesNavTmRepositoryTests : TmRepositoryBase
    {
        [Test]
        public void QueryRepository_IncludeNav_False_DoesNotPopulateClasses()
        {
            var (entity, _) = 
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

            testDbContext.TrademarkRegistrations.Add(entity);
            testDbContext.SaveChanges();

            var repository = 
                new TrademarkRepository(testDbContext);

            var result = 
                repository.Query(new TrademarkSearchFilter
            {
                SearchBy = TrademarkSearchBy.Wordmark,
                SearchTerm = "ALPHA",
                ExactMatch = true
            }, includeNav: false).Single();

            (result.Classes == null || result.Classes.Count == 0).
                Should().BeTrue();
        }

        [Test]
        public void QueryRepository_IncludeNav_True_PopulatesClasses()
        {
            var (entity1, _) = 
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

            var (entity2, _) = 
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
                entity1, 
                entity2);

            testDbContext.SaveChanges();

            var repository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                repository.Query(new TrademarkSearchFilter
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
