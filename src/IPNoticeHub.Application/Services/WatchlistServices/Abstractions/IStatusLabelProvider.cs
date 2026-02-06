namespace IPNoticeHub.Application.Services.WatchlistService.Abstractions
{
    public interface IStatusLabelProvider
    {
        string GetStatusLabel(string source, int code);
    }
}
