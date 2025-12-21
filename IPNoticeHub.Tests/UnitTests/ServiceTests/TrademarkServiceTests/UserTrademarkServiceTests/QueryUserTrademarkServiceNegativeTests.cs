using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class QueryUserTrademarkServiceNegativeTests
    {
        [Test]
        public async Task IsInCollectionAsync_WhenUserDoesNotExistInDbContext_ReturnsFalse()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(tmEntity);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = 
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var service = 
                new TrademarkCollectionService(
                    tmRepository, 
                    userTmRepository);

            var linkExists = await service.IsInCollectionAsync(
                "missing-user", 
                tmEntity.Id, 
                includeSoftDeleted: true,
                default);

            linkExists.Should().BeFalse();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenEmpty_ReturnsEmptyPage()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            testDbContext.Users.Add(user);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = 
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var service = 
                new TrademarkCollectionService(
                    tmRepository, 
                    userTmRepository);

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
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "newUser",
                Email = "user1@test.local"
            };

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25 });

            var (tmEntity2, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "ZZZ",
                owner: "Owner B",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "7654321",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 35 });

            testDbContext.Users.Add(user);

            testDbContext.TrademarkRegistrations.AddRange(
                tmEntity1, 
                tmEntity2);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository tmRepository = 
                new TrademarkRepository(testDbContext);

            IUserTrademarkRepository userTmRepository = 
                new UserTrademarkRepository(testDbContext);

            var service = 
                new TrademarkCollectionService(
                    tmRepository, 
                    userTmRepository);

            await service.AddAsync(
                user.Id, 
                tmEntity1.Id, 
                default);

            await service.AddAsync(
                user.Id, 
                tmEntity2.Id, 
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
