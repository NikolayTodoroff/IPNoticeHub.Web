namespace IPNoticeHub.Data.EnumConstants
{
    /// <summary>  
    /// Represents key procedural actions or timeline events in the trademark lifecycle.  
    /// Designed to be extensible without disrupting the core status categories.  
    /// </summary>
    public enum TrademarkEventType
    {
        ApplicationFiled = 1,
        AssignedToExaminingAttorney = 2,
        NonFinalOfficeActionMailed = 3,
        FinalOfficeActionMailed = 4,
        SuspensionInquiryMailed = 5,
        PublicationApproved = 6,
        PublishedForOpposition = 7,
        OppositionFiled = 8,
        OppositionTerminated = 9,
        ExtensionOfTimeToOpposeFiled = 10,
        NoticeOfAllowanceIssued = 11,
        StatementOfUseFiled = 12,
        RegistrationIssued = 13,
        Section8Accepted = 14,
        Section15Accepted = 15,
        Section8And15Accepted = 16,
        Section71Accepted = 17,
        RenewalAccepted = 18,
        Surrendered = 19,
        Revived = 20,
        Reinstated = 21,
        CancellationFiled = 22,
        CancellationGranted = 23,
        CourtOrderEntered = 24,
        Other = 25
    }
}
