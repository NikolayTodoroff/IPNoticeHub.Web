using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Common.EnumConstants
{
    /// <summary>
    /// Represents the criteria by which a trademark can be searched.
    /// </summary>
    public enum TrademarkSearchBy
    {
        [Display(Name = "Wordmark")] Wordmark = 0,
        [Display(Name = "Owner")] Owner = 1,
        [Display(Name = "Number")] Number = 2
    }
}
