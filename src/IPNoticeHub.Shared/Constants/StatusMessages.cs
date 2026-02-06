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

        public static class DmcaDocumentTemplates
        {
            public const string GoodFaithStatement =
                "I have a good faith belief that the disputed use of the " +
                "copyrighted material is not authorized by the copyright owner, " +
                "its agent, or the law.";

            public const string BodyTemplate = "Dear {{RecipientName}},\n\nI, {{SenderName}} ({{SenderEmail}}), " +
            "submit this DMCA notice concerning the work \"{{WorkTitle}}\" (Reg. No. {{RegistrationNumber}}). " +
            "The infringing material appears at {{InfringingUrl}}." +
            "\n\n{{GoodFaithStatement}}\n\nSincerely,\n{{SenderName}}\n{{SenderAddress}}";
        }

        public static class PreviewPageMessages
        {
            public const string NoDataMessage = "No preview data found. Please complete the form.";
            public const string SessionExpiredMessage = "Your preview session expired. Please re-enter your details.";
            public const string CopyrightDetailsMissingMessage = "Unable to load copyright details for preview.";
            public const string TrademarkDetailsMissingMessage = "Unable to load trademark details for preview.";
        }

        public static class EditPageMessages
        {
            public const string CeaseDesistSavedMessage = "Your Cease & Desist letter was successfully saved to your library.";
            public const string DmcaNoticeSavedMessage = "Your DMCA notice was successfully saved to your library.";
        }
    }
}
