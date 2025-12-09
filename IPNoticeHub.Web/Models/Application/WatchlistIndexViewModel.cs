using IPNoticeHub.Services.Watchlist.DTOs;
public sealed class WatchlistIndexViewModel
{
    public IReadOnlyList<TrademarkWatchlistItemDto> Items { get; init; } = Array.Empty<TrademarkWatchlistItemDto>();
}
