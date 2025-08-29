namespace IPNoticeHub.Common.EnumConstants
{
    /// <summary>
    /// Specifies the available sorting options for trademark and copyright collections.
    /// </summary>
    public enum CollectionSortBy
    {
        DateAddedDesc = 0, // 0: For both trademarks and copyrights
        DateAddedAsc = 1,  // 1: For both trademarks and copyrights
        WordmarkAsc = 2,   // 2: For trademarks
        TitleAsc = 2,      // 2: For copyrights
        WordmarkDesc = 3,  // 3: For trademarks
        TitleDesc = 3      // 3: For copyrights
    }
}
