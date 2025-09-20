using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Common.EnumConstants
{
    /// <summary>
    /// Represents the search modes used for filtering trademarks.
    /// </summary>
    public enum SearchMode
    {
        [Display(Name = "Identical")]
        Identical = 0,

        [Display(Name = "Contains")]
        Contains = 1
    }
}
