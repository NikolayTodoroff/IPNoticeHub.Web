namespace IPNoticeHub.Shared.EnumConstants
{
    /// <summary>
    /// Defines the various status categories for trademarks, representing their current state in the application or registration process.
    /// </summary>
    public enum TrademarkStatusCategory
    {
        Pending = 0,    // Represents a trademark application that is currently under review or awaiting action.
        Registered = 1, // Represents a trademark that has been successfully registered with the USPTO.
        Cancelled = 2,  // Represents a trademark registration that has been canceled, either voluntarily or due to non-compliance.
        Abandoned = 3   // Represents a trademark application or registration that has been abandoned and is no longer active.
    }
}
