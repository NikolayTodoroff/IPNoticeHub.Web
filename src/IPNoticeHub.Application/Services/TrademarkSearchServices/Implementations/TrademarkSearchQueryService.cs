using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkSearchService.Abstractions;
using IPNoticeHub.Shared.Support;

public sealed class TrademarkSearchQueryService : ITrademarkSearchQueryService
{
    private readonly ITrademarkReadRepository tmSearchServiceRepo;

    public TrademarkSearchQueryService(ITrademarkReadRepository tmSearchServiceRepository)
        => tmSearchServiceRepo = tmSearchServiceRepository;

    public async Task<(IReadOnlyList<TrademarkSearchResultDto> Items, int Total)> SearchAsync(
        TrademarkSearchQueryDto searchQuery,
        CancellationToken cancellationToken = default)
    {
        PagedResult<TrademarkSearchResultDto> pagedResult =
            await tmSearchServiceRepo.SearchAsync(searchQuery, cancellationToken);

        return (pagedResult.Results, pagedResult.ResultsCount);
    }
}
