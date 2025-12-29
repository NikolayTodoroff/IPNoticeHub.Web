using FluentAssertions;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class DetailsTmSearchServiceTests : TmSearchServiceBase
    {
        [Test]
        public async Task GetDetailsAsync_WhenPublicIdExists_ReturnsDetailsDto()
        {
            var (entity, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Test Wordmark",
                owner: "Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 35 });

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            var result = 
                await service.GetDetailsAsync(
                    entity.PublicId, 
                    default);

            result.Should().NotBeNull();
            result!.PublicId.Should().Be(entity.PublicId);
            result.Wordmark.Should().Be("AAA");
            result.Owner.Should().Be("Owner A");
            result.Classes.Should().Contain(new[] { 25, 35 });

            result.Provider.Should().
                BeOneOf(
                DataProvider.USPTO, 
                DataProvider.EUIPO, 
                DataProvider.WIPO); 
        }

        [Test]
        public async Task GetDetailsAsync_WhenPublicIdDoesNotExist_ReturnsNull()
        {
            var result = 
                await service.GetDetailsAsync(
                Guid.NewGuid(), 
                default);

            result.Should().BeNull();
        }

        [Test]
        public async Task GetDetailsAsync_WhenTrademarkHasEvents_MapsEventsDescendingByDate()
        {
            var (entity, _) =
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "Test Wordmark",
                owner: "Test Owner",
                goodsAndServices: "testGoodsAndSerices1",
                sourceId: "X123AZ",
                statusDetail: "Successfully Registered",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 25, 35 });

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();
            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(2020, 1, 1),
                Code = "E1", 
                Description = "First"
            });

            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(2021, 2, 2),
                Code = "E2", 
                Description = "Second"
            });

            await testDbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(
                entity.PublicId, 
                default);

            result.Should().NotBeNull();
            result!.Events.Should().HaveCount(2);

            result.Events.Select(
                e => e.Code).
                Should().ContainInOrder("E2", "E1");
        }
    }
}
