namespace IPNoticeHub.Shared.EnumConstants
{
    /// <summary>
    /// Represents various categories of trademark events.
    /// Each value corresponds to a specific type of event associated with trademark processing and management.
    /// </summary>
    public enum TrademarkEventType
    {
        Office = 0,       // O - USPTO issued something
        Incoming = 1,     // I - Applicant/attorney submitted something
        Electronic = 2,   // E - USPTO system generated (emails, reminders)
        Assignment = 3,   // A - Assignment events (ownership changes)
        Renewal = 4,      // R - Renewal/maintenance
        Publication = 5,  // P - Publication actions
        Deadline = 6,     // D - Deadlines, reminders
        Other = 99        // Fallback if code not recognized
    }
}
