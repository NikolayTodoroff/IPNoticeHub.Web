using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Shared.Enums
{
    public enum TrademarkSearchBy
    {
        [Display(Name = "Wordmark")] Wordmark = 0,
        [Display(Name = "Owner")] Owner = 1,
        [Display(Name = "Number")] Number = 2
    }
}
