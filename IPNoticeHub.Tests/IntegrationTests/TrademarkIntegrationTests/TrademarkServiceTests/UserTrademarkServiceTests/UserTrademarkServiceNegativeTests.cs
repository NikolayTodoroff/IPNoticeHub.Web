using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests.TrademarkServiceTests.UserTrademarkServiceTests
{
    public class UserTrademarkServiceNegativeTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task IsInCollectionAsync_WhenUserDoesNotExistInDbContext_ReturnsFalse()
        {
            var entity =
               InMemoryDbContextFactory.CreateTrademarkEntity(
               wordmark: "Test Wordmark",
               owner: "Test Owner",
               goodsAndServices: "testGoodsAndSerices1",
               sourceId: "X123AZ",
               statusDetail: "Successfully Registered",
               regNumber: "1234567",
               status: TrademarkStatusCategory.Registered,
               source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            var linkExists = await service.IsInCollectionAsync(
                "missing-user",
                entity.Id, 
                includeSoftDeleted: true,
                default);

            linkExists.Should().BeFalse();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenEmpty_ReturnsEmptyPage()
        {
            var pagedResult = 
                await service.GetUserCollectionAsync(
                user.Id, 
                currentPage: 1, 
                resultsPerPage: 10, 
                default);

            pagedResult.ResultsCount.Should().Be(0);
            pagedResult.Results.Should().BeEmpty();
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(10);
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenPageOrSizeInvalid_NormalizesWithoutThrowing()
        {
            var entity1 =
              InMemoryDbContextFactory.CreateTrademarkEntity(
              wordmark: "Test Wordmark A",
              owner: "Test Owner A",
              goodsAndServices: "testGoodsAndSerices A",
              sourceId: "X123AZ",
              statusDetail: "Successfully Registered",
              regNumber: "1234567",
              status: TrademarkStatusCategory.Registered,
              source: DataProvider.USPTO);

            var entity2 =
              InMemoryDbContextFactory.CreateTrademarkEntity(
              wordmark: "Test Wordmark B",
              owner: "Test Owner B",
              goodsAndServices: "testGoodsAndSerices B",
              sourceId: "Z246Y",
              statusDetail: "Successfully Registered",
              regNumber: "1234567",
              status: TrademarkStatusCategory.Registered,
              source: DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.AddRange(entity1,entity2);
            await testDbContext.SaveChangesAsync();

            await service.AddAsync(
                user.Id,
                entity1.Id, 
                default);

            await service.AddAsync(
                user.Id,
                entity2.Id, 
                default);

            var pageResult = 
                await service.GetUserCollectionAsync(
                user.Id, 
                currentPage: 0, 
                resultsPerPage: 0,
                default);

            pageResult.CurrentPage.Should().BeGreaterThan(0);
            pageResult.ResultsCountPerPage.Should().BeGreaterThan(0);
            pageResult.Results.Should().NotBeEmpty();
            pageResult.ResultsCount.Should().Be(2);
        }
    }
}
