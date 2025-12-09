using IPNoticeHub.Services.Watchlist.DTOs;

namespace IPNoticeHub.Web.Models.Watchlist;
public sealed class WatchlistIndexViewModel
{
    public IReadOnlyList<TrademarkWatchlistItemDto> Items { get; init; } = 
        Array.Empty<TrademarkWatchlistItemDto>();
}
