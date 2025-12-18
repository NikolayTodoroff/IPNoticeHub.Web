using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class TmSearchServiceGetDetailsAsyncTests
    {
        [Test]
        public async Task GetDetailsAsync_WhenPublicIdExists_ReturnsDetailsDto()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            testDbContext.TrademarkRegistrations.Add(tmEntity1);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var details = 
                await service.GetDetailsAsync(
                    tmEntity1.PublicId, 
                    default);

            details.Should().
                NotBeNull();

            details!.PublicId.Should().
                Be(tmEntity1.PublicId);

            details.Wordmark.Should().
                Be("AAA");

            details.Owner.Should().
                Be("Owner A");

            details.Provider.Should().
                BeOneOf(DataProvider.USPTO, DataProvider.EUIPO, DataProvider.WIPO);
            
            details.Classes.Should().
                Contain(new[] { 9, 25 });
        }

        [Test]
        public async Task GetDetailsAsync_WhenPublicIdDoesNotExist_ReturnsNull()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var details = await service.GetDetailsAsync(
                Guid.NewGuid(), 
                default);

            details.Should().
                BeNull();
        }

        [Test]
        public async Task GetDetailsAsync_WhenTrademarkHasEvents_MapsEventsDescendingByDate()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (entity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 35 });

            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(
                year: 2020, month: 1, day: 1),
                Code = "E1", Description = "First"
            });

            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(
                year: 2021, month: 2, day: 2),
                Code = "E2", Description = "Second"
            });

            testDbContext.TrademarkRegistrations.AddRange(entity);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var service = 
                new TrademarkSearchService(trademarkRepository);

            var details = await service.GetDetailsAsync(
                entity.PublicId, 
                default);

            details.Should().
                NotBeNull();

            details!.Events.Should().
                HaveCount(2);

            details.Events.Select(
                e => e.Code).Should().
                ContainInOrder("E2", "E1");
        }
    }
}
