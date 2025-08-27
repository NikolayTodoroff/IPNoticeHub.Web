using IPNoticeHub.Common.EnumConstants;
namespace IPNoticeHub.Services.DTOs.Trademarks
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();

        public int Total { get; init; }

        public int Page { get; init; }

        public int PageSize { get; init; }
    }
}
