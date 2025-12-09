using static IPNoticeHub.Common.ValidationConstants;

namespace IPNoticeHub.Common.Infrastructure.Paging
{
    public static class PagingConfiguration
    {
        public static (int Page, int PageSize) NormalizePaging(int page, int resultsPerPage)
        {
            page = page < PagingConstants.DefaultPage ? PagingConstants.DefaultPage : page;

            if (resultsPerPage < 1)
            {
                resultsPerPage = PagingConstants.DefaultPageSize;
            }

            else if (resultsPerPage > PagingConstants.MaxPageSize)
            {
                resultsPerPage = PagingConstants.MaxPageSize;
            }

            return (page, resultsPerPage);
        }
    }
}
