using IPNoticeHub.Services.Application.DTOs;
using static IPNoticeHub.Common.ValidationConstants.PagingConstants;
public sealed class WatchlistIndexViewModel
{
    public IReadOnlyList<TrademarkWatchlistItemDTO> Items { get; init; } = Array.Empty<TrademarkWatchlistItemDTO>();
}
