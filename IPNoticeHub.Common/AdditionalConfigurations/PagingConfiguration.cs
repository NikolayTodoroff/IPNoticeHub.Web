using static IPNoticeHub.Common.EntityValidationConstants;

namespace IPNoticeHub.Common.AdditionalConfigurations
{
    public static class PagingConfiguration
    {
        /// <summary>
        /// Sets the page and page size values, ensuring they fall within the defined constraints.
        /// </summary>
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
