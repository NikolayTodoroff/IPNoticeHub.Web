using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Data.EnumConstants
{
    /// <summary>
    /// Defines the various status categories for trademarks, representing their current state in the application or registration process.
    /// </summary>
    public enum TrademarkStatusCategory
    {
        /// <summary>
        /// Represents a trademark application that is currently under review or awaiting action.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Represents a trademark that has been successfully registered with the USPTO.
        /// </summary>
        Registered = 1,

        /// <summary>
        /// Represents a trademark registration that has been canceled, either voluntarily or due to non-compliance.
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Represents a trademark application or registration that has been abandoned and is no longer active.
        /// </summary>
        Abandoned = 3
    }
}
