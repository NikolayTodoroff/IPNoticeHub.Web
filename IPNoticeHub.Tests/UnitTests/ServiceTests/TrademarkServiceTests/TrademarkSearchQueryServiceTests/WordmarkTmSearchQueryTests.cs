using FluentAssertions;
using IPNoticeHub.Shared.Enums;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.TrademarkSearchQueryServiceTests
{
    public class WordmarkTmSearchQueryTests : TmSearchQueryBase
    {
        [Test]
        public async Task SearchAsync_WhenWordmarkContainsQuery_ReturnsMatchingTrademark()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "Osir",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult,total) = 
                await service.SearchAsync(dto,CancellationToken.None);

            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be("Osiris");
            queryItem.Owner.Should().Be("Afterlife Inc.");
            queryItem.RegistrationNumber.Should().Be("3355442");
            queryItem.Status.Should().Be(TrademarkStatusCategory.Cancelled.ToString());
            queryItem.Id.Should().Be(osirisTm.Id);
        }

        [Test]
        public async Task SearchAsync_WhenWordmarkIdenticalQuery_ReturnsMatchingTrademark()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "Anubis",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            queryResult.Should().ContainSingle();
            total.Should().Be(1);

            var queryItem = queryResult.Single();
            queryItem.Wordmark.Should().Be("Anubis");
            queryItem.Owner.Should().Be("Underworld Inc.");
            queryItem.RegistrationNumber.Should().Be("1234567");
            queryItem.Status.Should().Be(TrademarkStatusCategory.Registered.ToString());
            queryItem.Id.Should().Be(anubisTm.Id);
        }

        [Test]
        public async Task SearchAsync_WhenOwnerContainsQuery_ReturnsMatchingOwners()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "Inc",
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(2);
            queryResult.Select(i => i.Owner).
                Should().BeEquivalentTo(new[] { 
                    "Underworld Inc.", 
                    "Afterlife Inc." });
        }

        [Test]
        public async Task SearchAsync_WhenOwnerIdentityQuery_ReturnsOnlyMatchingOwners()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "Falcon LLC",
                SearchBy = TrademarkSearchBy.Owner,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);
            queryResult.Single().Owner.Should().Be("Falcon LLC");
        }

        [Test]
        public async Task SearchAsync_WhenNumberIdenticalQuery_MatchesOnlyExactRegistrationNumber()
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = "1234567",
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Identical,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);

            queryResult.Single().RegistrationNumber.
                Should().Be("1234567");
        }

        [Test]
        public async Task SearchAsync_WhenNumberContainsQuery_MatchesOnlyExactRegistrationNumber()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "12345",
                SearchBy = TrademarkSearchBy.Number,
                Mode = SearchMode.Contains,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(2);

            queryResult.Select(q => q.RegistrationNumber).
                Should().BeEquivalentTo(new[] { "1234567", "1234512" });
        }

        [Test]
        public async Task SearchAsync_WithStatusRegistered_FiltersOnlyRegistered()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Status = TrademarkStatusCategory.Registered,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(1);

            queryResult.Single().Wordmark.Should().Be("Anubis");
        }

        [Test]
        public async Task SearchAsync_WithClass25_FiltersOnlyItemsHavingClass25()
        {
            var dto = 
                new TrademarkSearchQueryDto
            {
                Query = "",
                SearchBy = TrademarkSearchBy.Wordmark,
                Mode = SearchMode.Contains,
                Class = (TrademarkClass)25,
                Page = DefaultPage,
                PageSize = DefaultPageSize
            };

            var (queryResult, total) = 
                await service.SearchAsync(dto, CancellationToken.None);

            total.Should().Be(2);
            queryResult.Select(q => q.Wordmark).
                Should().BeEquivalentTo(new[] { "Anubis", "Horus" });
        }
    }
}
