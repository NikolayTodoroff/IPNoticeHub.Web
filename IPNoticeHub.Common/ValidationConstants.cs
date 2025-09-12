namespace IPNoticeHub.Common
{
    public static class ValidationConstants
    {
        public static class TrademarkRegistrationConstants
        {
            public const int WordmarkMaxLength = 300;
            public const int SourceIdMaxLength = 50;
            public const int RegistrationNumberMaxLength = 50;
            public const int GoodsAndServicesMaxLength = 1000;
            public const int OwnerNameMaxLength = 255;
            public const int TrademarkStatusDetailsMaxLength = 500;
            public const int MarkImageUrlMaxLength = 2048;
            public const int FilterSearchTermMaxLength = 500;
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

        public static class PagingConstants
        {
            public const int DefaultPage = 1;
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 200;
        }

        public static class StatusMessages
        {
            public const string TrademarkAddedMessage = "TrademarkAdded added to your collection.";
            public const string TrademarkRemovedMessage = "TrademarkAdded removed from your collection.";

            public const string CopyrightAddedMessage = "Copyright added to your collection.";
            public const string CopyrightRemovedMessage = "Copyright removed from your collection.";
            public const string CopyrightUpdatesMessage = "Copyright updated.";
        }
    }
}
