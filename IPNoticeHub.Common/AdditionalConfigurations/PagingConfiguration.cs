using static IPNoticeHub.Common.EntityValidationConstants;

namespace IPNoticeHub.Common.AdditionalConfigurations
{
    public static class PagingConfiguration
    {
        /// <summary>
        /// Sets the page and page size values, ensuring they fall within the defined constraints.
        /// </summary>
        public static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
        {
            page = page < PagingConstants.DefaultPage ? PagingConstants.DefaultPage : page;

            if (pageSize < 1)
            {
                pageSize = PagingConstants.DefaultPageSize;
            }
            else if (pageSize > PagingConstants.MaxPageSize)
            {
                pageSize = PagingConstants.MaxPageSize;
            }

            return (page, pageSize);
        }
    }
}
