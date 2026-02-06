using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;

namespace IPNoticeHub.Shared.Support
{
    public static class PagingConfiguration
    {
        public static (int Page, int PageSize) NormalizePaging(int page, int resultsPerPage)
        {
            page = page < DefaultPage ? DefaultPage : page;

            if (resultsPerPage < 1)
            {
                resultsPerPage = DefaultPageSize;
            }

            else if (resultsPerPage > MaxPageSize)
            {
                resultsPerPage = MaxPageSize;
            }

            return (page, resultsPerPage);
        }
    }
}
