using IPNoticeHub.Application.DTOs.WatchlistDTOs;

namespace IPNoticeHub.Web.Models.Watchlist;
public sealed class WatchlistIndexViewModel
{
    public IReadOnlyList<WatchlistItemDto> Items { get; init; } = 
        Array.Empty<WatchlistItemDto>();
}
