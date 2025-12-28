using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests
{
    public class CopyrightServiceNegativeTests : CopyrightServiceBase
    {
        [Test]
        public async Task GetDetailsAsync_WhenPublicIdDoesNotExist_ReturnsNull()
        {
            var dto = await service.GetDetailsAsync(
                user.Id,
                Guid.NewGuid(),
                CancellationToken.None);

            dto.Should().BeNull();
        }

        [Test]
        public async Task GetDetailsAsync_WhenNotLinked_ReturnsNull()
        {
            var result = await service.GetDetailsAsync(
                user.Id,
                cpEntity1.PublicId,
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_WhenPublicIdMissing_ReturnsFalse()
        {
            var nonExistentId = Guid.NewGuid();
            
            var result = await service.RemoveAsync(
                user.Id, 
                nonExistentId, 
                CancellationToken.None);

            result.Should().BeFalse();
        }

        [Test]
        public async Task GetUserCollectionAsync_WhenEmpty_ReturnsEmptyPage()
        {
            var pagedResult = 
                await service.GetUserCollectionAsync(
                userId: user.Id,
                sortBy: Shared.Enums.CollectionSortBy.DateAddedDesc,
                page: 1,
                resultsPerPage: 10,
                cancellationToken: CancellationToken.None);

            pagedResult.ResultsCount.Should().Be(0);
            pagedResult.Results.Should().BeEmpty();
            pagedResult.CurrentPage.Should().Be(1);
            pagedResult.ResultsCountPerPage.Should().Be(10);
        }
    }
}
