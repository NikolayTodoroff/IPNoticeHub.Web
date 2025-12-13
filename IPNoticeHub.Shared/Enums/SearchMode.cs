using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Shared.Enums
{
    public enum SearchMode
    {
        [Display(Name = "Identical")]
        Identical = 0,

        [Display(Name = "Contains")]
        Contains = 1
    }
}
