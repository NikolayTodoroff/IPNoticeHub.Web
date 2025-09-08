using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Repositories.Trademarks.Abstractions;
using IPNoticeHub.Data.Repositories.Trademarks.Implementations;
using IPNoticeHub.Services.Trademarks.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.TrademarkSearchServiceTests
{
    public class TmSearchServiceGetDetailsAsyncTests
    {
        /// <summary>
        /// Section: TrademarkSearchService – GetDetailsAsync behaviour
        ///  - Returns results sorted by Wordmark and then by Id to maintain stable ordering.
        ///  - Provides accurate paging metadata, including ResultsCount, CurrentPage, and ResultsCountPerPage.
        /// </summary>
        [Test]
        public async Task GetDetailsAsync_WhenPublicIdExists_ReturnsDetailsDto()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            var (tmEntity1, _) = InMemoryDbContextFactory.CreateTrademark(
                wordmark: "AAA",
                owner: "Owner A",
                regNumber: "1234567",
                status: TrademarkStatusCategory.Registered,
                source: DataProvider.USPTO,
                classNumbers: new[] { 9, 25 });

            testDbContext.TrademarkRegistrations.Add(tmEntity1);

            await testDbContext.SaveChangesAsync();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var details = await service.GetDetailsAsync(tmEntity1.PublicId, default);

            details.Should().NotBeNull();
            details!.PublicId.Should().Be(tmEntity1.PublicId);
            details.Wordmark.Should().Be("AAA");
            details.Owner.Should().Be("Owner A");
            details.Provider.Should().BeOneOf(DataProvider.USPTO, DataProvider.EUIPO, DataProvider.WIPO);
            details.Classes.Should().Contain(new[] { 9, 25 });
        }

        [Test]
        public async Task GetDetailsAsync_WhenPublicIdDoesNotExist_ReturnsNull()
        {
            using var testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            ITrademarkRepository trademarkRepository = new TrademarkRepository(testDbContext);
            var service = new TrademarkSearchService(trademarkRepository);

            var details = await service.GetDetailsAsync(Guid.NewGuid(), default);

            details.Should().BeNull();
        }
    }
}
