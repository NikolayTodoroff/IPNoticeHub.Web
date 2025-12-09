namespace IPNoticeHub.Services.Watchlist.Abstractions
{
    public interface IStatusLabelProvider
    {
        string GetStatusLabel(string source, int code);
    }
}
