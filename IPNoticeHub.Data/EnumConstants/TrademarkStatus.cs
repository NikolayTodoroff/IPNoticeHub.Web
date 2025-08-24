using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Data.EnumConstants
{
    /// <summary>
    /// Represents a simplified lifecycle of USPTO trademark applications and registrations.
    /// Consolidates detailed TSDR status texts into broader categories for easier filtering and watchlist management.
    /// </summary>
    public enum TrademarkStatus
    {
        [Display(Name = "Unknown")] Unknown = 0,
        [Display(Name = "Pending")] Pending = 1,
        [Display(Name = "Examination")] Examination = 2,
        [Display(Name = "Office Action Issued")] OfficeActionIssued = 3,
        [Display(Name = "Final Office Action")] FinalOfficeAction = 4,
        [Display(Name = "Suspended")] Suspended = 5,
        [Display(Name = "Approved for Publication")] ApprovedForPublication = 6,
        [Display(Name = "Published for Opposition")] PublishedForOpposition = 7,
        [Display(Name = "Opposition")] Opposition = 8,
        [Display(Name = "Notice of Allowance")] Allowed = 9,
        [Display(Name = "Registered")] Registered = 10,
        [Display(Name = "Abandoned")] Abandoned = 20,
        [Display(Name = "Canceled")] Cancelled = 21,
        [Display(Name = "Expired")] Expired = 22
    }
}
