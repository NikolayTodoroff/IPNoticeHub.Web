namespace IPNoticeHub.Shared.Constants
{
    public static class StatusMessages
    {
        public static class TrademarkStatusMessages
        {
            public const string TmAddedToCollectionMessage = "Trademark added to your collection.";
            public const string TmRemovedFromCollectionMessage = "Trademark removed from your collection.";
            public const string TmAddToCollectionErrorMessage = "Trademark removed from your collection.";

            public const string TmAddedToWatchlistMessage = "TrademarkAdded added to your watchlist.";
            public const string TmRemovedFromWatchlistMessage = "TrademarkAdded removed from your watchlist.";
            public const string TmAlreadyInWatchlistMessage = "Trademark is already in your watchlist.";
            public const string TmAddToWatchlistErrorMessage = "Could not add to Watchlist.";
        }

        public static class CopyrightStatusMessages
        {
            public const string CopyrightAddedMessage = "Copyright added to your collection.";
            public const string CopyrightRemovedMessage = "Copyright removed from your collection.";
            public const string CopyrightUpdatesMessage = "Copyright updated.";
            public const string CopyrightUpdatesErrorMessage = "Unable to save changes. " +
                "Ensure you have this item in your collection and the registration number is unique.";
        }

        public static class EmailNotificationsStatusMessages
        {
            public const string EmailNotificationsEnabledMessage = "Email notifications enabled successfully.";
            public const string EmailNotificationsDisabledMessage = "Email notifications disabled successfully.";
            public const string EmailNotificationsErrorMessage = "Failed to toggle email notifications.";

            public const string ManualSyncTriggeredMessage = "Manual sync triggered. " +
                "Registry updates and watchlist checks will run shortly.";
        }
    }
}
