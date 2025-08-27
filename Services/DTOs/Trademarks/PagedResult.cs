using IPNoticeHub.Common.EnumConstants;
namespace IPNoticeHub.Services.DTOs.Trademarks
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Results { get; init; } = new List<T>();

        public int ResultsCount { get; init; }

        public int CurrentPage { get; init; }

        public int ResultsCountPerPage { get; init; }
    }
}
