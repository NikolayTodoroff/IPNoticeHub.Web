namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface IStatusLabelProvider
    {
        string GetStatusLabel(string source, int code);
    }
}
