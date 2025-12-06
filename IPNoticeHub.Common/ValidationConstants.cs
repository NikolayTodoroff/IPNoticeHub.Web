namespace IPNoticeHub.Common
{
    public static class ValidationConstants
    {
        public static class TrademarkRegistrationConstants
        {
            public const int WordmarkMaxLength = 300;
            public const int SourceIdMaxLength = 50;
            public const int RegistrationNumberMaxLength = 50;
            public const int GoodsAndServicesMaxLength = 4000;
            public const int OwnerNameMaxLength = 255;
            public const int TrademarkStatusDetailsMaxLength = 1000;
            public const int MarkImageUrlMaxLength = 2048;
            public const int FilterSearchTermMaxLength = 500;
            public const int WatchlistInitialStatusTextMaxLength = 1000;
        }

        public static class TrademarkEventConstants
        {
            public const int TrademarkEventCodeMaxLength = 10;
            public const int TrademarkEventDescriptionMaxLength = 500;
            public const int TrademarkEventTypeRawMaxLength = 2;
        }

        public static class TrademarkSearchFilterConstants
        {
            public const int TrademarkSearchTermMaxLength = 100;
            public const int TrademarkCurrentPageMinValue = 1;
            public const int TrademarkResultsPerPageMinValue = 20;
            public const int TrademarkResultsPerPageMaxValue = 100;
        }

        public static class CopyrightRegistrationConstants
        {
            public const int RegistrationNumberMaxLength = 20;
            public const int WorkTypeMaxLength = 50;
            public const int TitleMaxLength = 300;
            public const int OwnerNameMaxLength = 255;
            public const int NationOfFirstPublicationMaxLength = 100;
        }

        public static class LegalDocumentConstants
        {
            public const int LegalDocumentsTitleMaxLength = 256;
            public const int IpTitleMaxLength = 256;
            public const int RegistrationNumberMaxLength = 50;
        }

        public static class PagingConstants
        {
            public const int DefaultPage = 1;
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 200;
        }

        public static class StatusMessages
        {
            public const string TmAddedToCollectionMessage = "Trademark added to your collection.";
            public const string TmRemovedFromCollectionMessage = "Trademark removed from your collection.";
            public const string TmAddToCollectionErrorMessage = "Trademark removed from your collection.";

            public const string TmAddedToWatchlistMessage = "TrademarkAdded added to your watchlist.";
            public const string TmRemovedFromWatchlistMessage = "TrademarkAdded removed from your watchlist.";
            public const string TmAlreadyInWatchlistMessage = "Trademark is already in your watchlist.";
            public const string TmAddToWatchlistErrorMessage = "Could not add to Watchlist.";

            public const string CopyrightAddedMessage = "Copyright added to your collection.";
            public const string CopyrightRemovedMessage = "Copyright removed from your collection.";
            public const string CopyrightUpdatesMessage = "Copyright updated.";
            public const string CopyrightUpdatesErrorMessage = "Unable to save changes. " +
                "Ensure you have this item in your collection and the registration number is unique.";

            public const string EmailNotificationsEnabledMessage = "Email notifications enabled successfully.";
            public const string EmailNotificationsDisabledMessage = "Email notifications disabled successfully.";
            public const string EmailNotificationsErrorMessage = "Failed to toggle email notifications.";

            public const string ManualSyncTriggeredMessage = "Manual sync triggered. " +
                "Registry updates and watchlist checks will run shortly.";
        }

        public static class FormattingConstants
        {
            public static string DateTimeFormat = "dd MMM yyyy";
        }

        public static class AdminAccountCredentials
        {
            public const string AdminEmailAddress = "admin@ipnoticehub.com";
            public const string AdminEmailPassword = "Admin!234";
        }

        public static class AuthRedirectPaths
        {
            public const string LoginPath = "/Identity/Account/Login";
            public const string AccessDeniedPath = "/Identity/Account/AccessDenied";
        }
    }
}
