using FluentAssertions;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.Trademarks.UserTrademarkServiceTests
{
    public class UserTrademarkServiceNegativeTests : UserTrademarkServiceBase
    {
        [Test]
        public async Task IsInCollectionAsync_WhenUserDoesNotExistInDbContext_ReturnsFalse()
        {
            var linkExists = await service.IsInCollectionAsync(
                "missing-user",
                tmEntity1.Id, 
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
