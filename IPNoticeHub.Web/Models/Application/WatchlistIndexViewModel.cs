using IPNoticeHub.Services.Application.DTOs;
public sealed class WatchlistIndexViewModel
{
    public IReadOnlyList<TrademarkWatchlistItemDTO> Items { get; init; } = Array.Empty<TrademarkWatchlistItemDTO>();
}
