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
                    CancellationToken.None);

            result.Should().NotBeNull();
            result!.PublicId.Should().Be(entity.PublicId);
            result.Wordmark.Should().Be(entity.Wordmark);
            result.Owner.Should().Be(entity.Owner);
            result.Classes.Should().BeEquivalentTo(new[] { 25, 35 });

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
            const string firstEventCode = "Ev1";
            const string firstEventDescription = "First Event";

            const string secondEventCode = "Ev2";
            const string secondEventDescription = "Second Event";

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

            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(2020, 1, 1),
                Code = firstEventCode, 
                Description = firstEventDescription
            });

            entity.Events.Add(new TrademarkEvent
            {
                EventDate = new DateTime(2021, 2, 2),
                Code = secondEventCode, 
                Description = secondEventDescription
            });

            testDbContext.TrademarkRegistrations.Add(entity);
            await testDbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(
                entity.PublicId, 
                default);

            result.Should().NotBeNull();
            result!.Events.Should().HaveCount(2);

            result.Events.Select(
                e => e.Code).
                Should().ContainInOrder(secondEventCode, firstEventCode);
        }
    }
}
