using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.Trademarks.TrademarkRepositoryTests
{
    public class TmRepoLookupsTests
    {
        [Test]
        public async Task ExistsAsync_ReturnsTrue_WhenIdExists()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
               wordmark: "ALPHA",
               owner: "Owner",
               goodsAndServices: "testGoodsAndSerices",
               sourceId: "testSourceId",
               statusDetail: "testStatusDetail",
               regNumber: "1234567",
               TrademarkStatusCategory.Registered,
               DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepo = 
                new TrademarkRepository(testDbContext);

            var queryResult = await trademarkRepo.ExistsAsync(
                trademarkEntity.Id, 
                CancellationToken.None);

            queryResult.Should().
                BeTrue();
        }

        [Test]
        public async Task ExistsAsync_ReturnsFalse_WhenIdMissing()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = await trademarkRepository.ExistsAsync(
                987654, 
                CancellationToken.None);

            queryResult.Should().
                BeFalse();
        }

        [Test]
        public async Task GetByPublicIdAsync_ReturnsEntity_WithIncludes_WhenFound()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
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

            trademarkEntity.Events.Add(
                new TrademarkEvent
            {
                EventDate = DateTime.UtcNow.Date,
                Code= "1",
                EventType = TrademarkEventType.Publication,
                Description = "testDescription"
            });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                await trademarkRepository.GetByPublicIdAsync(
                    trademarkEntity.PublicId, 
                    asNoTracking: false, 
                    CancellationToken.None);

            queryResult.Should().
                NotBeNull();

            queryResult!.Wordmark.Should().
                Be("ALPHA");

            queryResult.Classes!.Select(
                c => c.ClassNumber).Should().
                BeEquivalentTo(new[] { 9, 25 });

            queryResult.Events!.Should().
                HaveCount(1);

            testDbContext.Entry(queryResult).State.Should().
                Be(EntityState.Unchanged);
        }

        [Test]
        public async Task GetByPublicIdAsync_AsNoTracking_True_ReturnsDetachedEntity()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "BETA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "2",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO,
                classNumbers: new[] { 25 });

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                await trademarkRepository.GetByPublicIdAsync(
                    trademarkEntity.PublicId, 
                    asNoTracking: true, 
                    CancellationToken.None);

            queryResult.Should().
                NotBeNull();

            testDbContext.Entry(queryResult!).State.Should().
                Be(EntityState.Detached);
        }

        [Test]
        public async Task GetByPublicIdAsync_ReturnsNull_WhenNotFound()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var queryResult = 
                await trademarkRepository.GetByPublicIdAsync(
                    Guid.NewGuid(), 
                    asNoTracking: true, 
                    CancellationToken.None);

            queryResult.Should().
                BeNull();
        }

        [Test]
        public async Task GetIdByPublicIdAsync_ReturnsId_WhenFound()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var (trademarkEntity, _) = 
                InMemoryDbContextFactory.CreateTrademark(
                wordmark: "DELTA",
                owner: "Owner",
                goodsAndServices: "testGoodsAndSerices",
                sourceId: "testSourceId",
                statusDetail: "testStatusDetail",
                regNumber: "1234567",
                TrademarkStatusCategory.Registered,
                DataProvider.USPTO);

            testDbContext.TrademarkRegistrations.Add(trademarkEntity);
            testDbContext.SaveChanges();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            int? id = 
                await trademarkRepository.GetIdByPublicIdAsync(
                trademarkEntity.PublicId, 
                CancellationToken.None);

            id.Should().Be(trademarkEntity.Id);
        }

        [Test]
        public async Task GetIdByPublicIdAsync_ReturnsNull_WhenNotFound()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            var id = 
                await trademarkRepository.GetIdByPublicIdAsync(
                    Guid.NewGuid(), 
                    CancellationToken.None);

            id.Should().
                BeNull();
        }

        [Test]
        public async Task ExistsAsync_Throws_WhenCancellationRequested()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var trademarkRepository = 
                new TrademarkRepository(testDbContext);

            using var cancellationToken = new CancellationTokenSource();
            cancellationToken.Cancel();

            await trademarkRepository.Invoking(
                r => r.ExistsAsync(id: 1, cancellationToken.Token)).Should().
                ThrowAsync<OperationCanceledException>();
        }
    }
}
